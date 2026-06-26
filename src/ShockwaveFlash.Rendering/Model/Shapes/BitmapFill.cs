using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record BitmapFill(IImage Bitmap, Matrix Matrix, bool Smoothed, bool Repeat) : IFillStyle
{
    public IFillStyle TransformColors(ColorTransform colorTransform)
    {
        return new BitmapFill(Bitmap.TransformColors(colorTransform), Matrix, Smoothed, Repeat);
    }
}
