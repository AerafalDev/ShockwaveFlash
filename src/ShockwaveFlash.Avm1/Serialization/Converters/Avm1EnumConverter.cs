using System.Globalization;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1EnumConverter<T> : Avm1Converter
    where T : struct, Enum
{
    public override Type Type => typeof(T);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        return Enum.ToObject(typeof(T), (long)value.AsNumber);
    }

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        return new Avm1Number(Convert.ToInt64(value, CultureInfo.InvariantCulture));
    }
}
