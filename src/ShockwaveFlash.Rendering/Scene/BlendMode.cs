namespace ShockwaveFlash.Rendering.Scene;

public enum BlendMode
{
    Normal = 1,
    Layer = 2,
    Multiply = 3,
    Screen = 4,
    Lighten = 5,
    Darken = 6,
    Difference = 7,
    Add = 8,
    Subtract = 9,
    Invert = 10,
    Alpha = 11,
    Erase = 12,
    Overlay = 13,
    HardLight = 14
}

public static class BlendModeExtensions
{
    public static string? ToCssMixBlendMode(this BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Multiply => "multiply",
            BlendMode.Screen => "screen",
            BlendMode.Lighten or BlendMode.Add => "lighten",
            BlendMode.Darken or BlendMode.Subtract => "darken",
            BlendMode.Difference => "difference",
            BlendMode.Overlay => "overlay",
            BlendMode.HardLight => "hard-light",
            _ => null
        };
    }
}
