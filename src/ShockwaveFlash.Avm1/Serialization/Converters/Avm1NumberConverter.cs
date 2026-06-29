using System.Globalization;
using System.Numerics;
using ShockwaveFlash.Avm1.Exceptions;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Serialization.Converters;

internal sealed class Avm1NumberConverter<T> : Avm1Converter
    where T : INumberBase<T>
{
    public override Type Type => typeof(T);

    internal override object? ReadBoxed(Avm1Value value, Avm1SerializerOptions options)
    {
        return T.CreateTruncating(ReadDouble(value, options));
    }

    internal override Avm1Value WriteBoxed(object? value, Avm1SerializerOptions options)
    {
        var number = double.CreateTruncating((T)value!);

        if ((options.NumberHandling & Avm1NumberHandling.WriteAsString) != 0)
            return new Avm1String(number.ToString(CultureInfo.InvariantCulture));

        return new Avm1Number(number);
    }

    private static double ReadDouble(Avm1Value value, Avm1SerializerOptions options)
    {
        if (value is Avm1Number number)
            return number.Value;

        if (value is Avm1String text && (options.NumberHandling & Avm1NumberHandling.AllowReadingFromString) != 0)
        {
            if ((options.NumberHandling & Avm1NumberHandling.AllowNamedFloatingPointLiterals) != 0)
            {
                switch (text.Value)
                {
                    case "NaN": return double.NaN;
                    case "Infinity": return double.PositiveInfinity;
                    case "-Infinity": return double.NegativeInfinity;
                }
            }

            if (double.TryParse(text.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
        }

        throw new Avm1SerializationException($"Cannot read a number of type '{typeof(T)}' from {value.GetType().Name}.");
    }
}
