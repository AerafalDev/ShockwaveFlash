using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using ShockwaveFlash.Avm1.Serialization.Converters;

namespace ShockwaveFlash.Avm1.Serialization;

public sealed class Avm1SerializerOptions
{
    private static readonly Avm1SerializerOptions DefaultOptions = new();

    private readonly ConcurrentDictionary<Type, Avm1Converter> _cache = new();

    public IList<Avm1Converter> Converters { get; } = [];

    public Avm1NumberHandling NumberHandling { get; set; }

    public Avm1IgnoreCondition DefaultIgnoreCondition { get; set; } = Avm1IgnoreCondition.WhenWritingNull;

    public bool IncludeFields { get; set; }

    public static Avm1SerializerOptions Default => DefaultOptions;

    internal Avm1Converter GetConverter(Type type) => _cache.GetOrAdd(type, BuildConverter);

    internal Avm1Converter GetConverterFromAttribute(Type converterType, Type targetType)
    {
        var converter = (Avm1Converter)Activator.CreateInstance(converterType)!;
        return converter is Avm1ConverterFactory factory ? factory.CreateConverter(targetType, this) : converter;
    }

    private Avm1Converter BuildConverter(Type type)
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

        if (type is { IsArray: true } && type.GetArrayRank() == 1)
            return Make(typeof(Avm1ArrayConverter<>), type.GetElementType()!);

        if (Avm1TypeShape.TryGetDictionaryValue(type, out var valueType))
            return Make(typeof(Avm1DictionaryConverter<>), valueType);

        if (Avm1TypeShape.TryGetEnumerableElement(type, out var elementType))
            return Make(typeof(Avm1ListConverter<>), elementType);

        return (Avm1Converter)Activator.CreateInstance(typeof(Avm1ObjectConverter<>).MakeGenericType(type), this)!;
    }

    private static Avm1Converter Make(Type openConverter, Type argument)
    {
        return (Avm1Converter)Activator.CreateInstance(openConverter.MakeGenericType(argument))!;
    }
}
