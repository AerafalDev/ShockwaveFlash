using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Scene;

public sealed record Frame(Rectangle Bounds, IReadOnlyList<FrameObject> Objects)
{
    public void Draw(IDrawer drawer, int frame)
    {
        drawer.Area(Bounds);

        foreach (var item in Objects)
        {
            if (item.ClipDepth is not null)
                continue;

            var drawable = item.ColorTransform is { } colorTransform
                ? item.Drawable.TransformColors(colorTransform)
                : item.Drawable;

            drawer.Include(drawable, item.Matrix, frame, item.Filters, item.BlendMode, item.Name);
        }
    }

    public Frame TransformColors(ColorTransform colorTransform)
    {
        return this with { Objects = [.. Objects.Select(item => item with { Drawable = item.Drawable.TransformColors(colorTransform) })] };
    }
}
