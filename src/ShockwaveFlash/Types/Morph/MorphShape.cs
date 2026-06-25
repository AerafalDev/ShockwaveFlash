using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Types.Morph;

public sealed record MorphShape(Rectangle ShapeBounds, Rectangle EdgeBounds, FillStyle[] FillStyles, LineStyle[] LineStyles, IReadOnlyList<ShapeRecord> Shapes);
