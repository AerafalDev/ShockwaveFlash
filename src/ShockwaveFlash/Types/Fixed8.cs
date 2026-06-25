using System.Globalization;

namespace ShockwaveFlash.Types;

public readonly record struct Fixed8(short Raw)
{
    public static Fixed8 Zero => new(0);

    public static Fixed8 One => new(1 << 8);

    public float ToSingle()
    {
        return Raw / 256f;
    }

    public double ToDouble()
    {
        return Raw / 256.0;
    }

    public static Fixed8 FromSingle(float value)
    {
        return new Fixed8((short)Math.Round(value * 256.0));
    }

    public override string ToString()
    {
        return ToDouble().ToString(CultureInfo.InvariantCulture);
    }
}
