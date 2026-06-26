using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record Path(IReadOnlyList<IEdge> Edges, PathStyle Style)
{
    public Path TransformColors(ColorTransform colorTransform)
    {
        return this with { Style = Style.TransformColors(colorTransform) };
    }
}
