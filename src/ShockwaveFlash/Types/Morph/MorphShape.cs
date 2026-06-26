using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Types.Morph;

public sealed class MorphShape
{
    public Rectangle ShapeBounds { get; set; }

    public Rectangle EdgeBounds { get; set; }

    public FillStyle[] FillStyles { get; set; }

    public LineStyle[] LineStyles { get; set; }

    public IReadOnlyList<ShapeRecord> Shapes { get; set; }

    public MorphShape(Rectangle shapeBounds, Rectangle edgeBounds, FillStyle[] fillStyles, LineStyle[] lineStyles, IReadOnlyList<ShapeRecord> shapes)
    {
        ShapeBounds = shapeBounds;
        EdgeBounds = edgeBounds;
        FillStyles = fillStyles;
        LineStyles = lineStyles;
        Shapes = shapes;
    }
}
