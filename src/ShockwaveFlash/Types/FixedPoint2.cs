namespace ShockwaveFlash.Types;

public readonly record struct FixedPoint2(Fixed16 X, Fixed16 Y)
{
    public static FixedPoint2 Zero => new(Fixed16.Zero, Fixed16.Zero);
}
