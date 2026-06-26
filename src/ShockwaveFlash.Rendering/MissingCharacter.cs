using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering;

public sealed class MissingCharacter : IDrawable
{
    public static MissingCharacter Instance { get; } = new();

    public Rectangle Bounds => new(0, 0, 0, 0);

    public int FrameCount(bool recursive = false)
    {
        return 1;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return this;
    }
}
