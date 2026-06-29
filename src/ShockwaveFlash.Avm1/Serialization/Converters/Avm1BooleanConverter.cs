using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1BooleanConverter : Avm1Converter
{
    public override Type Type => typeof(bool);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        return value.AsBoolean;
    }


    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        return new Avm1Boolean((bool)value!);
    }

}
