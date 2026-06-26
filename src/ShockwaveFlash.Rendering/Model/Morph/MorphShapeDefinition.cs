using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Processing;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;
using ShockwaveFlash.Types.Shape.Gradients;
using CoreGradient = ShockwaveFlash.Types.Shape.Gradients.Gradient;
using CoreMorphShape = ShockwaveFlash.Types.Morph.MorphShape;

namespace ShockwaveFlash.Rendering.Model.Morph;

// Ports Ruffle's MorphShapeShared::build_morph_frame: interpolate the start/end fills, lines and
// shape records at a ratio, then tessellate the resulting static shape.
public sealed class MorphShapeDefinition : IDrawable
{
    private const int MaxRatio = 65535;

    private readonly ShapeProcessor _processor;

    private readonly CoreMorphShape _start;

    private readonly CoreMorphShape _end;

    private readonly Dictionary<int, ShapeDefinition> _cache = new();

    public Rectangle Bounds => _start.ShapeBounds;

    public MorphShapeDefinition(ShapeProcessor processor, CoreMorphShape start, CoreMorphShape end)
    {
        _processor = processor;
        _start = start;
        _end = end;
    }

    public int FrameCount(bool recursive = false)
    {
        return 1;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        return AtRatio(0).Draw(drawer, frame);
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return AtRatio(0).TransformColors(colorTransform);
    }

    public ShapeDefinition AtRatio(int ratio)
    {
        ratio = Math.Clamp(ratio, 0, MaxRatio);

        if (_cache.TryGetValue(ratio, out var cached))
            return cached;

        var b = ratio / (float)MaxRatio;
        var a = 1f - b;

        var fills = new FillStyle[_start.FillStyles.Length];
        for (var i = 0; i < fills.Length; i++)
            fills[i] = LerpFill(_start.FillStyles[i], _end.FillStyles[i], a, b);

        var lines = new LineStyle[_start.LineStyles.Length];
        for (var i = 0; i < lines.Length; i++)
            lines[i] = new LineStyle(LerpInt(_start.LineStyles[i].Width, _end.LineStyles[i].Width, a, b), LerpFill(_start.LineStyles[i].FillStyle, _end.LineStyles[i].FillStyle, a, b), _start.LineStyles[i].Flags, _start.LineStyles[i].MiterLimit);

        var bounds = LerpRectangle(_start.ShapeBounds, _end.ShapeBounds, a, b);
        var shape = _processor.ProcessMorph(BuildRecords(a, b), fills, lines, bounds);
        var definition = ShapeDefinition.FromShape(shape, bounds);

        _cache[ratio] = definition;
        return definition;
    }

    private List<ShapeRecord> BuildRecords(float a, float b)
    {
        var records = new List<ShapeRecord>();
        var startList = _start.Shapes;
        var endList = _end.Shapes;
        var si = 0;
        var ei = 0;
        var startPen = new Point(0, 0);
        var endPen = new Point(0, 0);

        while (si < startList.Count && ei < endList.Count)
        {
            var s = startList[si];
            var e = endList[ei];

            if (s is EndShapeRecord || e is EndShapeRecord)
                break;

            switch (s, e)
            {
                case (StyleChangeRecord startChange, StyleChangeRecord endChange):
                    if (startChange.MoveTo is { } startMove)
                        startPen = startMove;

                    if (endChange.MoveTo is { } endMove)
                        endPen = endMove;

                    var bothMove = startChange.MoveTo is not null || endChange.MoveTo is not null;
                    records.Add(startChange with { MoveTo = bothMove ? LerpPoint(startPen, endPen, a, b) : startChange.MoveTo });
                    si++;
                    ei++;
                    break;

                case (StyleChangeRecord onlyStart, _):
                    var startOnlyMove = onlyStart.MoveTo;
                    if (onlyStart.MoveTo is { } move)
                    {
                        startPen = move;
                        startOnlyMove = LerpPoint(startPen, endPen, a, b);
                    }

                    records.Add(onlyStart with { MoveTo = startOnlyMove });
                    startPen = UpdatePen(startPen, s);
                    si++;
                    break;

                case (_, StyleChangeRecord onlyEnd):
                    var endOnlyMove = onlyEnd.MoveTo;
                    if (onlyEnd.MoveTo is { } emove)
                    {
                        endPen = emove;
                        endOnlyMove = LerpPoint(startPen, endPen, a, b);
                    }

                    records.Add(onlyEnd with { MoveTo = endOnlyMove });
                    endPen = UpdatePen(endPen, s);
                    ei++;
                    break;

                default:
                    records.Add(LerpEdge(startPen, endPen, s, e, a, b));
                    startPen = UpdatePen(startPen, s);
                    endPen = UpdatePen(endPen, e);
                    si++;
                    ei++;
                    break;
            }
        }

        records.Add(new EndShapeRecord());
        return records;
    }

    private static Point UpdatePen(Point pen, ShapeRecord record)
    {
        return record switch
        {
            StraightEdgeRecord straight => new Point(pen.X + straight.Delta.X, pen.Y + straight.Delta.Y),
            CurvedEdgeRecord curved => new Point(pen.X + curved.ControlDelta.X + curved.AnchorDelta.X, pen.Y + curved.ControlDelta.Y + curved.AnchorDelta.Y),
            StyleChangeRecord { MoveTo: { } move } => move,
            _ => pen
        };
    }

    private static ShapeRecord LerpEdge(Point startPen, Point endPen, ShapeRecord start, ShapeRecord end, float a, float b)
    {
        var pen = LerpPoint(startPen, endPen, a, b);

        switch (start, end)
        {
            case (StraightEdgeRecord ss, StraightEdgeRecord es):
                var sa = Add(startPen, ss.Delta);
                var ea = Add(endPen, es.Delta);
                var anchor = LerpPoint(sa, ea, a, b);
                return new StraightEdgeRecord(Sub(anchor, pen));

            case (CurvedEdgeRecord sc, CurvedEdgeRecord ec):
                var sControl = Add(startPen, sc.ControlDelta);
                var sAnchor = Add(sControl, sc.AnchorDelta);
                var eControl = Add(endPen, ec.ControlDelta);
                var eAnchor = Add(eControl, ec.AnchorDelta);
                var control = LerpPoint(sControl, eControl, a, b);
                var anchor2 = LerpPoint(sAnchor, eAnchor, a, b);
                return new CurvedEdgeRecord(Sub(control, pen), Sub(anchor2, control));

            case (StraightEdgeRecord ss2, CurvedEdgeRecord ec2):
                var sControl2 = Add(startPen, Half(ss2.Delta));
                var sAnchor2 = Add(startPen, ss2.Delta);
                var eControl2 = Add(endPen, ec2.ControlDelta);
                var eAnchor2 = Add(eControl2, ec2.AnchorDelta);
                var control2 = LerpPoint(sControl2, eControl2, a, b);
                var anchor3 = LerpPoint(sAnchor2, eAnchor2, a, b);
                return new CurvedEdgeRecord(Sub(control2, pen), Sub(anchor3, control2));

            case (CurvedEdgeRecord sc3, StraightEdgeRecord es3):
                var sControl3 = Add(startPen, sc3.ControlDelta);
                var sAnchor3 = Add(sControl3, sc3.AnchorDelta);
                var eControl3 = Add(endPen, Half(es3.Delta));
                var eAnchor3 = Add(endPen, es3.Delta);
                var control3 = LerpPoint(sControl3, eControl3, a, b);
                var anchor4 = LerpPoint(sAnchor3, eAnchor3, a, b);
                return new CurvedEdgeRecord(Sub(control3, pen), Sub(anchor4, control3));

            default:
                return start;
        }
    }

    private static FillStyle LerpFill(FillStyle start, FillStyle end, float a, float b)
    {
        return (start, end) switch
        {
            (FillStyleSolid s, FillStyleSolid e) => new FillStyleSolid(LerpColor(s.Color, e.Color, a, b)),
            (FillStyleBitmap s, FillStyleBitmap e) => new FillStyleBitmap(s.Id, LerpMatrix(s.Matrix, e.Matrix, a, b), s.IsSmoothed, s.IsRepeating),
            (FillStyleLinearGradient s, FillStyleLinearGradient e) => new FillStyleLinearGradient(LerpGradient(s.Gradient, e.Gradient, a, b)),
            (FillStyleRadialGradient s, FillStyleRadialGradient e) => new FillStyleRadialGradient(LerpGradient(s.Gradient, e.Gradient, a, b)),
            (FillStyleFocalGradient s, FillStyleFocalGradient e) => new FillStyleFocalGradient(LerpGradient(s.Gradient, e.Gradient, a, b), Fixed8.FromSingle((s.FocalPoint.ToSingle() * a) + (e.FocalPoint.ToSingle() * b))),
            _ => start
        };
    }

    private static CoreGradient LerpGradient(CoreGradient start, CoreGradient end, float a, float b)
    {
        var records = new GradientRecord[start.Records.Length];

        for (var i = 0; i < records.Length; i++)
            records[i] = new GradientRecord((byte)((start.Records[i].Ratio * a) + (end.Records[i].Ratio * b)), LerpColor(start.Records[i].Color, end.Records[i].Color, a, b));

        return new CoreGradient(LerpMatrix(start.Matrix, end.Matrix, a, b), start.Spread, start.Interpolation, records);
    }

    private static Matrix LerpMatrix(Matrix start, Matrix end, float a, float b)
    {
        return new Matrix(
            new FixedPoint2(Fixed16.FromSingle((start.Scale.X.ToSingle() * a) + (end.Scale.X.ToSingle() * b)), Fixed16.FromSingle((start.Scale.Y.ToSingle() * a) + (end.Scale.Y.ToSingle() * b))),
            new FixedPoint2(Fixed16.FromSingle((start.Rotation.X.ToSingle() * a) + (end.Rotation.X.ToSingle() * b)), Fixed16.FromSingle((start.Rotation.Y.ToSingle() * a) + (end.Rotation.Y.ToSingle() * b))),
            new Point(LerpInt(start.Translation.X, end.Translation.X, a, b), LerpInt(start.Translation.Y, end.Translation.Y, a, b)));
    }

    private static Color LerpColor(Color start, Color end, float a, float b)
    {
        return new Color(
            (byte)Math.Clamp((int)((start.R * a) + (end.R * b)), 0, 255),
            (byte)Math.Clamp((int)((start.G * a) + (end.G * b)), 0, 255),
            (byte)Math.Clamp((int)((start.B * a) + (end.B * b)), 0, 255),
            (byte)Math.Clamp((int)((start.A * a) + (end.A * b)), 0, 255));
    }

    private static Rectangle LerpRectangle(Rectangle start, Rectangle end, float a, float b)
    {
        return new Rectangle(LerpInt(start.XMin, end.XMin, a, b), LerpInt(start.XMax, end.XMax, a, b), LerpInt(start.YMin, end.YMin, a, b), LerpInt(start.YMax, end.YMax, a, b));
    }

    private static Point LerpPoint(Point start, Point end, float a, float b)
    {
        return new Point(LerpInt(start.X, end.X, a, b), LerpInt(start.Y, end.Y, a, b));
    }

    private static int LerpInt(int start, int end, float a, float b)
    {
        return (int)Math.Round((start * a) + (end * b));
    }

    private static Point Add(Point p, Point delta) => new(p.X + delta.X, p.Y + delta.Y);

    private static Point Sub(Point p, Point other) => new(p.X - other.X, p.Y - other.Y);

    private static Point Half(Point delta) => new(delta.X / 2, delta.Y / 2);
}
