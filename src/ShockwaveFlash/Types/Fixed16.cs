using System.Globalization;

namespace ShockwaveFlash.Types;

public readonly record struct Fixed16(int Raw)
{
    public static Fixed16 Zero => new(0);

    public static Fixed16 One => new(1 << 16);

    public float ToSingle()
    {
        return Raw / 65536f;
    }

    public double ToDouble()
    {
        return Raw / 65536.0;
    }

    public static Fixed16 FromSingle(float value)
    {
        return new Fixed16((int)Math.Round(value * 65536.0));
    }

    public static Fixed16 FromDouble(double value)
    {
        return new Fixed16((int)Math.Round(value * 65536.0));
    }

    public override string ToString()
    {
        return ToDouble().ToString(CultureInfo.InvariantCulture);
    }
}
