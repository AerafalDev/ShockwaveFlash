using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1ObjectConverter<T> : Avm1Converter
{
    private readonly Avm1SerializerOptions _options;
    private readonly object _gate = new();
    private PropertyPlan[]? _properties;
    private ConstructorInfo? _constructor;
    private string[] _constructorArguments = Array.Empty<string>();

    public Avm1ObjectConverter(Avm1SerializerOptions options)
    {
        _options = options;
    }

    public override Type Type => typeof(T);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        EnsurePlan();

        if (value is not Avm1Object table)
            throw new Avm1SerializationException($"Cannot deserialize '{typeof(T)}' from {value.GetType().Name}.");

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in _properties!)
            values[property.MemberName] = ReadMember(property, table, options);

        object instance;
        if (_constructor is not null)
        {
            var arguments = new object?[_constructorArguments.Length];
            for (var i = 0; i < arguments.Length; i++)
                arguments[i] = values[_constructorArguments[i]];
            instance = _constructor.Invoke(arguments);
        }
        else
        {
            instance = Activator.CreateInstance<T>()!;
        }

        foreach (var property in _properties!)
            if (property.Settable && !property.IsConstructorParameter)
                property.SetValue(instance, values[property.MemberName]);

        return instance;
    }

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        EnsurePlan();

        var table = new Avm1Object();
        var instance = value!;

        var condition = options.DefaultIgnoreCondition;

        foreach (var property in _properties!)
        {
            var member = property.GetValue(instance);

            if (member is null)
            {
                if (condition is Avm1IgnoreCondition.WhenWritingNull or Avm1IgnoreCondition.WhenWritingDefault or Avm1IgnoreCondition.Always)
                    continue;

                table.Members[property.Key] = Avm1Value.Null;
                continue;
            }

            if (condition == Avm1IgnoreCondition.Always)
                continue;
            if (condition == Avm1IgnoreCondition.WhenWritingDefault && property.IsValueScalar && member.Equals(Activator.CreateInstance(property.UnderlyingType)))
                continue;

            table.Members[property.Key] = property.Converter.WriteBoxed(member, options);
        }

        return table;
    }

    private static object? ReadMember(PropertyPlan property, Avm1Object table, Avm1SerializerOptions options)
    {
        table.Members.TryGetValue(property.Key, out var value);
        value ??= Avm1Value.Undefined;

        if (property.Nullable)
            return value.IsNull || value.IsUndefined ? null : property.Converter.ReadBoxed(value, options);

        if (value.IsNull || value.IsUndefined)
        {
            if (property.ThrowIfMissing)
                throw new Avm1SerializationException($"AVM1 object is missing required member '{property.Key}'.");
            if (property.IsValueScalar)
                return Activator.CreateInstance(property.UnderlyingType);

            return property.Converter.ReadBoxed(Avm1Value.Undefined, options);
        }

        return property.Converter.ReadBoxed(value, options);
    }

    private void EnsurePlan()
    {
        if (_properties is not null)
            return;

        lock (_gate)
        {
            if (_properties is not null)
                return;

            var type = typeof(T);
            var constructors = type
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(c => IsAccessible(c.Attributes) && !IsCopyConstructor(type, c))
                .ToList();

            var isRecord = type.GetMethod("<Clone>$", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is not null;
            var parameterized = constructors.Where(c => c.GetParameters().Length > 0).ToList();
            var parameterless = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

            ConstructorInfo? constructor;
            if (isRecord && parameterized.Count > 0)
                constructor = parameterized.OrderByDescending(c => c.GetParameters().Length).First();
            else if (parameterless is not null || type.IsValueType)
                constructor = null;
            else if (parameterized.Count == 1)
                constructor = parameterized[0];
            else
                throw new Avm1SerializationException($"Type '{type}' has no usable constructor.");

            var parameterNames = constructor?.GetParameters().Select(p => p.Name!).ToArray() ?? Array.Empty<string>();
            var nullability = new NullabilityInfoContext();
            var properties = new List<PropertyPlan>();
            var keys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var (member, memberName, memberType, settable) in EnumerateMembers(type))
            {
                var isConstructorParameter = constructor is not null
                    && parameterNames.Any(p => string.Equals(p, memberName, StringComparison.OrdinalIgnoreCase));

                if (!isConstructorParameter && !settable)
                    continue;

                var key = member.GetCustomAttribute<Avm1PropertyAttribute>()?.Key ?? memberName;
                if (!keys.Add(key))
                    continue;

                var attribute = member.GetCustomAttribute<Avm1ConverterAttribute>();
                var converter = attribute is not null
                    ? _options.GetConverterFromAttribute(attribute.ConverterType, memberType)
                    : _options.GetConverter(memberType);

                var (nullable, throwIfMissing, isValueScalar, underlying) = Classify(memberType, member, nullability, attribute is not null);

                properties.Add(new PropertyPlan(key, memberName, member, converter, settable, isConstructorParameter, nullable, throwIfMissing, isValueScalar, underlying));
            }

            if (constructor is not null)
            {
                foreach (var name in parameterNames)
                {
                    if (!properties.Any(p => string.Equals(p.MemberName, name, StringComparison.OrdinalIgnoreCase)))
                        throw new Avm1SerializationException($"Constructor parameter '{name}' of '{type}' has no matching member.");
                }

                _constructorArguments = parameterNames;
            }

            _constructor = constructor;
            _properties = properties.ToArray();
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

    private sealed class PropertyPlan
    {
        private readonly MemberInfo _member;

        public PropertyPlan(string key, string memberName, MemberInfo member, Avm1Converter converter, bool settable, bool isConstructorParameter, bool nullable, bool throwIfMissing, bool isValueScalar, Type underlying)
        {
            Key = key;
            MemberName = memberName;
            _member = member;
            Converter = converter;
            Settable = settable;
            IsConstructorParameter = isConstructorParameter;
            Nullable = nullable;
            ThrowIfMissing = throwIfMissing;
            IsValueScalar = isValueScalar;
            UnderlyingType = underlying;
        }

        public string Key { get; }
        public string MemberName { get; }
        public Avm1Converter Converter { get; }
        public bool Settable { get; }
        public bool IsConstructorParameter { get; }
        public bool Nullable { get; }
        public bool ThrowIfMissing { get; }
        public bool IsValueScalar { get; }
        public Type UnderlyingType { get; }

        public object? GetValue(object instance) =>
            _member is PropertyInfo property ? property.GetValue(instance) : ((FieldInfo)_member).GetValue(instance);

        public void SetValue(object instance, object? value)
        {
            if (_member is PropertyInfo property)
                property.SetValue(instance, value);
            else
                ((FieldInfo)_member).SetValue(instance, value);
        }
    }
}
