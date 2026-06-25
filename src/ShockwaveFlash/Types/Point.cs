namespace ShockwaveFlash.Types;

public readonly record struct Point(int X, int Y)
{
    public static Point Zero => new(0, 0);

    public static Point One => new(1, 1);

    public static Point UnitX => new(1, 0);

    public static Point UnitY => new(0, 1);

    public bool IsZero => X is 0 && Y is 0;

    public Point(int value) : this(value, value)
    {
    }
}
