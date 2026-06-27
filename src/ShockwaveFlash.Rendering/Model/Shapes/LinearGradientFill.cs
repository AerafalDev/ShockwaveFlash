using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record LinearGradientFill(Matrix Matrix, Gradient Gradient) : IFillStyle
{
    public IFillStyle TransformColors(ColorTransform colorTransform)
    {
        return new LinearGradientFill(Matrix, Gradient.TransformColors(colorTransform));
    }
}
