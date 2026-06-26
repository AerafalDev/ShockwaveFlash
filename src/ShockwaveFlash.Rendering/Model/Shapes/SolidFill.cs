using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record SolidFill(Color Color) : IFillStyle
{
    public IFillStyle TransformColors(ColorTransform colorTransform)
    {
        return new SolidFill(colorTransform.Transform(Color));
    }
}
