using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record PathStyle
{
    public IFillStyle? Fill { get; init; }

    public Color? LineColor { get; init; }

    public IFillStyle? LineFill { get; init; }

    public int LineWidth { get; init; }

    public LineCapStyle LineCap { get; init; } = LineCapStyle.Round;

    public LineJoinStyle? LineJoin { get; init; }

    public float MiterLimit { get; init; } = 3f;

    public bool IsLine => LineColor is not null || LineFill is not null;

    public bool IsEmpty => Fill is null && LineWidth == 0;

    public PathStyle TransformColors(ColorTransform colorTransform)
    {
        return this with
        {
            Fill = Fill?.TransformColors(colorTransform),
            LineColor = LineColor is { } color ? colorTransform.Transform(color) : null,
            LineFill = LineFill?.TransformColors(colorTransform)
        };
    }
}
