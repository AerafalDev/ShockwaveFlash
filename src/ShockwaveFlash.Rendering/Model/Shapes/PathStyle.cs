using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed record PathStyle
{
    public IFillStyle? Fill { get; init; }

    public Color? LineColor { get; init; }

    public IFillStyle? LineFill { get; init; }

    public int LineWidth { get; init; }

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
