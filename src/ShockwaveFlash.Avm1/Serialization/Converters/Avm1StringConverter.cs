using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1StringConverter : Avm1Converter
{
    public override Type Type => typeof(string);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options) => value.AsString;

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options) => new Avm1String((string)value!);
}
