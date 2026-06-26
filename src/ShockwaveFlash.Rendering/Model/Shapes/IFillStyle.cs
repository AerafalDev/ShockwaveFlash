using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public interface IFillStyle
{
    IFillStyle TransformColors(ColorTransform colorTransform);
}
