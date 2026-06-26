using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record Shape(int Width, int Height, int XOffset, int YOffset, IReadOnlyList<ShapePath> Paths)
{
    public Shape TransformColors(ColorTransform colorTransform)
    {
        return this with { Paths = [.. Paths.Select(path => path.TransformColors(colorTransform))] };
    }
}
