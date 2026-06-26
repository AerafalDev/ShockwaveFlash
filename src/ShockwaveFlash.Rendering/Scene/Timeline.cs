using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Scene;

public sealed record Timeline(Rectangle Bounds, IReadOnlyList<Frame> Frames) : IDrawable
{
    [ThreadStatic]
    private static int s_depth;

    public int FrameCount(bool recursive = false)
    {
        var count = Math.Max(Frames.Count, 1);

        if (!recursive || s_depth >= 16)
            return count;

        s_depth++;

        try
        {
            foreach (var frame in Frames)
                foreach (var item in frame.Objects)
                    count = Math.Max(count, item.Drawable.FrameCount(true));
        }
        finally
        {
            s_depth--;
        }

        return count;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        if (Frames.Count is 0)
            return drawer;

        var index = ((frame % Frames.Count) + Frames.Count) % Frames.Count;

        Frames[index].Draw(drawer, frame);
        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return this with { Frames = [.. Frames.Select(frame => frame.TransformColors(colorTransform))] };
    }
}
