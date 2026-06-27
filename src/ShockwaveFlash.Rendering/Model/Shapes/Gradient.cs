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
    float? FocalPoint = null)
{
    public Gradient TransformColors(ColorTransform colorTransform)
    {
        var stops = Stops.Select(stop => stop with { Color = colorTransform.Transform(stop.Color) }).ToList();
        return this with { Stops = stops };
    }
}
