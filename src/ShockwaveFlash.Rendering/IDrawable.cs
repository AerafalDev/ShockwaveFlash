using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering;

public interface IDrawable
{
    Rectangle Bounds { get; }

    int FrameCount(bool recursive = false);

    IDrawer Draw(IDrawer drawer, int frame = 0);

    IDrawable TransformColors(ColorTransform colorTransform);
}
