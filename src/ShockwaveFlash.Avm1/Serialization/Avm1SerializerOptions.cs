using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Serialization.Converters;
using ShockwaveFlash.Avm1.Serialization.Metadata;

namespace ShockwaveFlash.Avm1.Serialization;

public sealed class Avm1SerializerOptions
{
    private static readonly Avm1SerializerOptions s_defaultOptions = new();
    private static readonly DefaultAvm1TypeInfoResolver s_defaultResolver = new();

    public static Avm1SerializerOptions Default =>
        s_defaultOptions;

    private readonly ConcurrentDictionary<Type, Avm1TypeInfo> _typeInfos = new();

    public IList<Avm1Converter> Converters { get; } = [];

    public IList<Action<Avm1TypeInfo>> Modifiers { get; } = [];

    public IAvm1TypeInfoResolver? TypeInfoResolver { get; set; }

    public Avm1NumberHandling NumberHandling { get; set; }

    public Avm1NamingPolicy? PropertyNamingPolicy { get; set; }

    public Avm1IgnoreCondition DefaultIgnoreCondition { get; set; } = Avm1IgnoreCondition.WhenWritingNull;

    internal Avm1TypeInfo GetTypeInfo(Type type)
    {
        return _typeInfos.GetOrAdd(type, BuildTypeInfo);
    }

    internal Avm1Converter GetConverter(Type type)
    {
        return GetTypeInfo(type).Converter;
    }

    internal Avm1Converter GetConverterFromAttribute(Type converterType, Type targetType)
    {
        var converter = (Avm1Converter)Activator.CreateInstance(converterType)!;
        return converter is Avm1ConverterFactory factory ? factory.CreateConverter(targetType, this) : converter;
    }

    internal Avm1Converter ResolveConverter(Type type)
    {
        foreach (var converter in Converters)
        {
            if (converter.CanConvert(type))
                return converter is Avm1ConverterFactory factory ? factory.CreateConverter(type, this) : converter;
        }

        var attribute = type.GetCustomAttribute<Avm1ConverterAttribute>();
        if (attribute is not null)
            return GetConverterFromAttribute(attribute.ConverterType, type);

        return BuildBuiltIn(type);
    }

    private Avm1TypeInfo BuildTypeInfo(Type type)
    {
        var resolver = TypeInfoResolver ?? s_defaultResolver;
        return resolver.GetTypeInfo(type, this)
            ?? throw new Avm1SerializationException($"No AVM1 metadata was found for type '{type}'.");
    }

    private Avm1Converter BuildBuiltIn(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return new Avm1NullableConverter(type, GetConverter(underlying));

        if (type == typeof(string))
            return new Avm1StringConverter();

        if (type == typeof(bool))
            return new Avm1BooleanConverter();

        if (Avm1TypeShape.IsPassthrough(type))
            return new Avm1PassthroughConverter(type);

        if (type.IsEnum)
            return Make(typeof(Avm1EnumConverter<>), type);

        if (Avm1TypeShape.IsNumeric(type))
            return Make(typeof(Avm1NumberConverter<>), type);

        if (type is { IsArray: true } && type.GetArrayRank() is 1)
            return Make(typeof(Avm1ArrayConverter<>), type.GetElementType()!);

        if (Avm1TypeShape.TryGetDictionaryValue(type, out var valueType))
            return Make(typeof(Avm1DictionaryConverter<>), valueType);

        if (Avm1TypeShape.TryGetEnumerableElement(type, out var elementType))
            return Make(typeof(Avm1ListConverter<>), elementType);

        return Make(typeof(Avm1ObjectConverter<>), type);
    }

    private static Avm1Converter Make(Type openConverter, Type argument)
    {
        return (Avm1Converter)Activator.CreateInstance(openConverter.MakeGenericType(argument))!;
    }
}
