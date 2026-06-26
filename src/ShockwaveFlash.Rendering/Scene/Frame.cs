using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Scene;

public sealed record Frame(Rectangle Bounds, IReadOnlyList<FrameObject> Objects)
{
    public void Draw(IDrawer drawer, int frame)
    {
        drawer.Area(Bounds);

        var clips = new List<(int ClipDepth, string Id)>();

        foreach (var item in Objects)
        {
            while (clips.Count > 0 && clips[^1].ClipDepth < item.Depth)
            {
                drawer.EndClip(clips[^1].Id);
                clips.RemoveAt(clips.Count - 1);
            }

            if (item.ClipDepth is { } clipDepth)
            {
                clips.Add((clipDepth, drawer.StartClip(item.Drawable, item.Matrix, frame)));
                continue;
            }

            var drawable = item.ColorTransform is { } colorTransform
                ? item.Drawable.TransformColors(colorTransform)
                : item.Drawable;

            drawer.Include(drawable, item.Matrix, frame, item.Filters, item.BlendMode, item.Name);
        }

        for (var i = clips.Count - 1; i >= 0; i--)
            drawer.EndClip(clips[i].Id);
    }

    public Frame TransformColors(ColorTransform colorTransform)
    {
        return this with { Objects = [.. Objects.Select(item => item with { Drawable = item.Drawable.TransformColors(colorTransform) })] };
    }
}
