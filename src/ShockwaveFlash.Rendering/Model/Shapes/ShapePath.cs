using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record ShapePath(IReadOnlyList<IEdge> Edges, PathStyle Style)
{
    public ShapePath TransformColors(ColorTransform colorTransform)
    {
        return this with { Style = Style.TransformColors(colorTransform) };
    }
}
