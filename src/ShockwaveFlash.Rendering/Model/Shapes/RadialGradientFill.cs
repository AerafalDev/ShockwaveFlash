using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record RadialGradientFill(Matrix Matrix, Gradient Gradient) : IFillStyle
{
    public IFillStyle TransformColors(ColorTransform colorTransform)
    {
        return new RadialGradientFill(Matrix, Gradient.TransformColors(colorTransform));
    }
}
