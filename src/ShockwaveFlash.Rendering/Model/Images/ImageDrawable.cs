using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Images;

public sealed record ImageDrawable(IImage Image) : IDrawable
{
    public Rectangle Bounds => Image.Bounds;

    public int FrameCount(bool recursive = false)
    {
        return 1;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Image(Image);
        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return new ImageDrawable(Image.TransformColors(colorTransform));
    }
}
