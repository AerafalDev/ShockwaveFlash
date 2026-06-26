using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public enum GradientSpread
{
    Pad,
    Reflect,
    Repeat
}

public enum GradientInterpolation
{
    Rgb,
    LinearRgb
}

public readonly record struct GradientStop(byte Ratio, Color Color);

public sealed record Gradient(
    GradientSpread Spread,
    GradientInterpolation Interpolation,
    IReadOnlyList<GradientStop> Stops,
    float? FocalPoint = null);
