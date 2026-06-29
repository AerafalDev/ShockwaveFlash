using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Serialization.Converters;

namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public sealed class DefaultAvm1TypeInfoResolver : IAvm1TypeInfoResolver
{
    public Avm1TypeInfo? GetTypeInfo(Type type, Avm1SerializerOptions options)
    {
        var converter = options.ResolveConverter(type);
        var info = CreateTypeInfo(type);
        info.Converter = converter;
        info.Kind = KindOf(converter);
        info.Options = options;

        var globalName = type.GetCustomAttribute<Avm1ObjectAttribute>()?.GlobalName;
        if (!string.IsNullOrEmpty(globalName))
            info.BindingPath = globalName!.Split('.');

        if (info.Kind is Avm1TypeInfoKind.Object)
            info.SetPopulate(target => Populate(target, type, options));

        return info;
    }

    private static Avm1TypeInfo CreateTypeInfo(Type type)
    {
        return (Avm1TypeInfo)Activator.CreateInstance(typeof(Avm1TypeInfo<>).MakeGenericType(type))!;
    }

    private static Avm1TypeInfoKind KindOf(Avm1Converter converter)
    {
        var type = converter.GetType();
        if (!type.IsGenericType)
            return Avm1TypeInfoKind.None;

        var definition = type.GetGenericTypeDefinition();
        if (definition == typeof(Avm1ObjectConverter<>))
            return Avm1TypeInfoKind.Object;
        if (definition == typeof(Avm1ListConverter<>) || definition == typeof(Avm1ArrayConverter<>))
            return Avm1TypeInfoKind.List;
        if (definition == typeof(Avm1DictionaryConverter<>))
            return Avm1TypeInfoKind.Dictionary;

        return Avm1TypeInfoKind.None;
    }

    private static void Populate(Avm1TypeInfo info, Type type, Avm1SerializerOptions options)
    {
        var constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(c => IsAccessible(c.Attributes) && !IsCopyConstructor(type, c))
            .ToList();

        var isRecord = type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is not null;
        var parameterized = constructors.Where(c => c.GetParameters().Length > 0).ToList();
        var parameterless = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

        ConstructorInfo? constructor;
        var marked = constructors.FirstOrDefault(c => c.GetCustomAttribute<Avm1ConstructorAttribute>() is not null);
        if (marked is not null)
            constructor = marked.GetParameters().Length == 0 ? null : marked;
        else if (isRecord && parameterized.Count > 0)
            constructor = parameterized.OrderByDescending(c => c.GetParameters().Length).First();
        else if (parameterless is not null || type.IsValueType)
            constructor = null;
        else if (parameterized.Count == 1)
            constructor = parameterized[0];
        else
            throw new Avm1SerializationException($"Type '{type}' has no usable constructor.");

        var parameterNames = constructor?.GetParameters().Select(p => p.Name!).ToArray() ?? [];
        var nullability = new NullabilityInfoContext();
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (member, memberName, memberType, settable) in EnumerateMembers(type))
        {
            var isConstructorParameter = constructor is not null
                && parameterNames.Any(p => string.Equals(p, memberName, StringComparison.OrdinalIgnoreCase));

            if (!isConstructorParameter && !settable && member.GetCustomAttribute<Avm1IncludeAttribute>() is null)
                continue;

            var explicitKey = member.GetCustomAttribute<Avm1PropertyAttribute>()?.Key;
            var key = explicitKey ?? (options.PropertyNamingPolicy?.ConvertName(memberName) ?? memberName);
            if (!keys.Add(key))
                continue;

            var attribute = member.GetCustomAttribute<Avm1ConverterAttribute>();
            var converter = attribute is not null
                ? options.GetConverterFromAttribute(attribute.ConverterType, memberType)
                : options.GetConverter(memberType);

            var (nullable, throwIfMissing, isValueScalar, underlying) = Classify(memberType, member, nullability, attribute is not null);
            var required = throwIfMissing || member.GetCustomAttribute<Avm1RequiredAttribute>() is not null;
            var order = member.GetCustomAttribute<Avm1PropertyOrderAttribute>()?.Order ?? 0;
            var isExtensionData = member.GetCustomAttribute<Avm1ExtensionDataAttribute>() is not null;

            info.Properties.Add(new Avm1PropertyInfo
            {
                Name = key,
                MemberName = memberName,
                Get = BuildGetter(member),
                Set = settable ? BuildSetter(member) : null,
                Converter = converter,
                Nullable = nullable,
                ThrowIfMissing = required,
                IsValueScalar = isValueScalar,
                UnderlyingType = underlying,
                IsConstructorParameter = isConstructorParameter,
                Settable = settable,
                IsRequired = required,
                Order = order,
                IsExtensionData = isExtensionData,
            });
        }

        if (constructor is not null)
        {
            foreach (var name in parameterNames)
            {
                if (!info.Properties.Any(p => string.Equals(p.MemberName, name, StringComparison.OrdinalIgnoreCase)))
                    throw new Avm1SerializationException($"Constructor parameter '{name}' of '{type}' has no matching member.");
            }

            info.ConstructorArguments = parameterNames;
            info.ConstructorFactory = arguments => constructor.Invoke(arguments);
        }
        else
        {
            info.ObjectFactory = () => Activator.CreateInstance(type)!;
        }
    }

    private static IEnumerable<(MemberInfo Member, string Name, Type Type, bool Settable)> EnumerateMembers(Type type)
    {
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (property.GetIndexParameters().Length > 0 || property.GetMethod is null || !IsAccessible(property.GetMethod.Attributes))
                continue;
            if (property.GetCustomAttribute<Avm1IgnoreAttribute>() is not null)
                continue;

            var settable = property.SetMethod is not null && IsAccessible(property.SetMethod.Attributes);
            yield return (property, property.Name, property.PropertyType, settable);
        }

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.IsLiteral || field.Name.Contains('<', StringComparison.Ordinal) || !IsFieldAccessible(field))
                continue;
            if (field.GetCustomAttribute<Avm1IgnoreAttribute>() is not null)
                continue;

            yield return (field, field.Name, field.FieldType, !field.IsInitOnly);
        }
    }

    private static (bool Nullable, bool ThrowIfMissing, bool IsValueScalar, Type Underlying) Classify(Type memberType, MemberInfo member, NullabilityInfoContext nullability, bool hasCustomConverter)
    {
        var underlying = Nullable.GetUnderlyingType(memberType) ?? memberType;
        var nullable = Nullable.GetUnderlyingType(memberType) is not null
            || (!memberType.IsValueType && IsNullableReference(member, nullability));

        if (hasCustomConverter)
            return (nullable, !nullable, false, underlying);

        if (underlying == typeof(string) || Avm1TypeShape.IsPassthrough(underlying))
            return (nullable, !nullable, false, underlying);

        if (underlying == typeof(bool) || underlying.IsEnum || Avm1TypeShape.IsNumeric(underlying))
            return (nullable, false, !nullable, underlying);

        if (Avm1TypeShape.IsCollection(underlying))
            return (nullable, false, false, underlying);

        return (nullable, !nullable, false, underlying);
    }

    private static bool IsNullableReference(MemberInfo member, NullabilityInfoContext nullability)
    {
        var info = member is PropertyInfo property ? nullability.Create(property) : nullability.Create((FieldInfo)member);
        return info.ReadState == NullabilityState.Nullable || info.WriteState == NullabilityState.Nullable;
    }

    private static Func<object, object?> BuildGetter(MemberInfo member)
    {
        if (member is PropertyInfo property)
            return instance => property.GetValue(instance);

        var field = (FieldInfo)member;
        return instance => field.GetValue(instance);
    }

    private static Action<object, object?> BuildSetter(MemberInfo member)
    {
        if (member is PropertyInfo property)
            return (instance, value) => property.SetValue(instance, value);

        var field = (FieldInfo)member;
        return (instance, value) => field.SetValue(instance, value);
    }

    private static bool IsCopyConstructor(Type type, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        return parameters.Length == 1 && parameters[0].ParameterType == type;
    }

    private static bool IsAccessible(MethodAttributes attributes)
    {
        var access = attributes & MethodAttributes.MemberAccessMask;
        return access is MethodAttributes.Public or MethodAttributes.Assembly or MethodAttributes.FamORAssem;
    }

    private static bool IsFieldAccessible(FieldInfo field)
    {
        var access = field.Attributes & FieldAttributes.FieldAccessMask;
        return access is FieldAttributes.Public or FieldAttributes.Assembly or FieldAttributes.FamORAssem;
    }
}
