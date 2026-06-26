using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Scene;

public sealed record Timeline(Rectangle Bounds, IReadOnlyList<Frame> Frames) : IDrawable
{
    public int FrameCount(bool recursive = false)
    {
        return Math.Max(Frames.Count, 1);
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        if (Frames.Count is 0)
            return drawer;

        Frames[Math.Clamp(frame, 0, Frames.Count - 1)].Draw(drawer, frame);
        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return this with { Frames = [.. Frames.Select(frame => frame.TransformColors(colorTransform))] };
    }
}
