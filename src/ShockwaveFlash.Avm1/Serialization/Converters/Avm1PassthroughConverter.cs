using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1PassthroughConverter : Avm1Converter
{
    public Avm1PassthroughConverter(Type type)
    {
        Type = type;
    }

    public override Type Type { get; }

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        return value;
    }


    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        return (Avm1Value)value!;
    }

}
