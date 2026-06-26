using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record LinearGradientFill(Matrix Matrix, Gradient Gradient) : IFillStyle
{
    public IFillStyle TransformColors(ColorTransform colorTransform)
    {
        var stops = Gradient.Stops.Select(stop => stop with { Color = colorTransform.Transform(stop.Color) }).ToList();
        return new LinearGradientFill(Matrix, Gradient with { Stops = stops });
    }
}
