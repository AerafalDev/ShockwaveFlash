using ShockwaveFlash.Rendering.Diagnostics;
using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;
using CoreGradient = ShockwaveFlash.Types.Shape.Gradients.Gradient;

namespace ShockwaveFlash.Rendering.Processing;

public sealed class ShapeProcessor
{
    private readonly IImageResolver? _images;

    private readonly RenderOptions _options;

    public ShapeProcessor(IImageResolver? images = null, RenderOptions? options = null)
    {
        _images = images;
        _options = options ?? new RenderOptions();
    }

    public Shape Process(DefineShapeTag tag)
    {
        var bounds = tag.ShapeBounds;
        var paths = ProcessRecords(tag.Shapes, tag.Styles.FillStyles, tag.Styles.LineStyles);

        return new Shape(bounds.Width, bounds.Height, -bounds.XMin, -bounds.YMin, paths);
    }

    private IReadOnlyList<ShapePath> ProcessRecords(IReadOnlyList<ShapeRecord> records, FillStyle[] fillStyles, LineStyle[] lineStyles)
    {
        var x = 0;
        var y = 0;
        StyleSlot? fill0 = null;
        StyleSlot? fill1 = null;
        StyleSlot? line = null;
        var group = 0;
        var builder = new PathsBuilder();
        var edges = new List<IEdge>();

        foreach (var record in records)
        {
            switch (record)
            {
                case StraightEdgeRecord straight:
                    {
                        var toX = x + straight.Delta.X;
                        var toY = y + straight.Delta.Y;
                        edges.Add(new StraightEdge(x, y, toX, toY));
                        x = toX;
                        y = toY;
                        break;
                    }

                case CurvedEdgeRecord curved:
                    {
                        var controlX = x + curved.ControlDelta.X;
                        var controlY = y + curved.ControlDelta.Y;
                        var toX = controlX + curved.AnchorDelta.X;
                        var toY = controlY + curved.AnchorDelta.Y;
                        edges.Add(new CurvedEdge(x, y, controlX, controlY, toX, toY));
                        x = toX;
                        y = toY;
                        break;
                    }

                case StyleChangeRecord change:
                    builder.Merge(fill0, fill1, line, edges);
                    edges.Clear();

                    if (change.NewStyles is { } newStyles)
                    {
                        builder.Close();
                        fillStyles = newStyles.FillStyles;
                        lineStyles = newStyles.LineStyles;
                        group++;
                    }

                    if (change.FillStyle0 is { } fillStyle0)
                        fill0 = fillStyle0 is 0 ? null : BuildFill(fillStyles, fillStyle0, group, true);

                    if (change.FillStyle1 is { } fillStyle1)
                        fill1 = fillStyle1 is 0 ? null : BuildFill(fillStyles, fillStyle1, group, false);

                    if (change.LineStyle is { } lineStyle)
                        line = lineStyle is 0 ? null : BuildLine(lineStyles, lineStyle, group);

                    if (change.MoveTo is { } moveTo)
                    {
                        x = moveTo.X;
                        y = moveTo.Y;
                    }

                    break;

                case EndShapeRecord:
                    builder.Merge(fill0, fill1, line, edges);
                    return builder.Export();
            }
        }

        builder.Merge(fill0, fill1, line, edges);
        return builder.Export();
    }

    private StyleSlot? BuildFill(FillStyle[] styles, uint index, int group, bool reverse)
    {
        if (index > (uint)styles.Length)
        {
            Report($"Fill style index {index} is out of range.");
            return null;
        }

        var style = new PathStyle { Fill = MapFill(styles[index - 1]) };
        return new StyleSlot(style, reverse, $"F{group}-{index}", group);
    }

    private StyleSlot? BuildLine(LineStyle[] styles, uint index, int group)
    {
        if (index > (uint)styles.Length)
        {
            Report($"Line style index {index} is out of range.");
            return null;
        }

        var lineStyle = styles[index - 1];
        var solid = lineStyle.FillStyle as FillStyleSolid;

        var style = new PathStyle
        {
            LineColor = solid?.Color,
            LineFill = solid is null ? MapFill(lineStyle.FillStyle) : null,
            LineWidth = lineStyle.Width,
            LineCap = lineStyle.StartCap,
            LineJoin = lineStyle.JoinStyle,
            MiterLimit = lineStyle.MiterLimit.ToSingle()
        };

        return new StyleSlot(style, false, $"L{group}-{index}", group);
    }

    private IFillStyle MapFill(FillStyle fillStyle)
    {
        switch (fillStyle)
        {
            case FillStyleSolid solid:
                return new SolidFill(solid.Color);

            case FillStyleLinearGradient linear:
                return new LinearGradientFill(linear.Gradient.Matrix, MapGradient(linear.Gradient, null));

            case FillStyleRadialGradient radial:
                return new RadialGradientFill(radial.Gradient.Matrix, MapGradient(radial.Gradient, null));

            case FillStyleFocalGradient focal:
                return new RadialGradientFill(focal.Gradient.Matrix, MapGradient(focal.Gradient, focal.FocalPoint.ToSingle()));

            case FillStyleBitmap bitmap:
                var image = _images?.ResolveImage(bitmap.Id);
                if (image is null)
                {
                    Report($"Bitmap {bitmap.Id} could not be resolved; using a transparent placeholder.");
                    return new SolidFill(new Color(0, 0, 0, 0));
                }

                return new BitmapFill(image, bitmap.Matrix, bitmap.IsSmoothed, bitmap.IsRepeating);

            default:
                Report($"Fill style {fillStyle.GetType().Name} is not rendered yet; using a transparent placeholder.");
                return new SolidFill(new Color(0, 0, 0, 0));
        }
    }

    private static Gradient MapGradient(CoreGradient gradient, float? focalPoint)
    {
        var stops = gradient.Records.Select(record => new GradientStop(record.Ratio, record.Color)).ToList();
        return new Gradient(GradientSpread.Pad, GradientInterpolation.Rgb, stops, focalPoint);
    }

    private void Report(string message)
    {
        _options.Diagnostics?.Report(new RenderDiagnostic(RenderSeverity.Warning, message));
    }
}
