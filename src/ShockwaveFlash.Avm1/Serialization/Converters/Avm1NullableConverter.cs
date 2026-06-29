using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1NullableConverter : Avm1Converter
{
    private readonly Avm1Converter _inner;

    public Avm1NullableConverter(Type nullableType, Avm1Converter inner)
    {
        Type = nullableType;
        _inner = inner;
    }

    public override Type Type { get; }

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        return value.IsNull || value.IsUndefined ? null : _inner.ReadBoxed(value, options);
    }

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        return value is null ? Avm1Value.Null : _inner.WriteBoxed(value, options);
    }
}
