using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.Control;
using ShockwaveFlash.Tags.DisplayList;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.DisplayList;
using ShockwaveFlash.Types.Filter;

namespace ShockwaveFlash.Rendering.Processing;

public sealed class TimelineProcessor
{
    private static readonly Matrix Identity = new(
        new FixedPoint2(Fixed16.One, Fixed16.One),
        new FixedPoint2(Fixed16.Zero, Fixed16.Zero),
        new Point(0, 0));

    private readonly ICharacterResolver _characters;

    private readonly RenderOptions _options;

    public TimelineProcessor(ICharacterResolver characters, RenderOptions? options = null)
    {
        _characters = characters;
        _options = options ?? new RenderOptions();
    }

    public Timeline Process(IReadOnlyList<Tag> tags)
    {
        var objects = new Dictionary<int, FrameObject>();
        var frames = new List<Frame>();

        foreach (var tag in tags)
        {
            if (tag is EndTag)
                break;

            switch (tag)
            {
                case PlaceObjectTag place:
                    objects[place.Depth] = NewObject(place.Depth, place.Id, place.Matrix, place.ColorTransform, null, null, null, []);
                    break;

                case PlaceObject2Tag place2:
                    Apply(objects, place2.Action, place2.Depth, place2.Matrix, place2.ColorTransform, (int?)place2.Ratio, place2.Name, (int?)place2.ClipDepth, []);
                    break;

                case PlaceObject3Tag place3:
                    Apply(objects, place3.Action, place3.Depth, place3.Matrix, place3.ColorTransform, (int?)place3.Ratio, place3.Name, (int?)place3.ClipDepth, place3.Filters ?? []);
                    break;

                case RemoveObject2Tag remove2:
                    objects.Remove(remove2.Depth);
                    break;

                case RemoveObjectTag remove:
                    objects.Remove(remove.Depth);
                    break;

                case ShowFrameTag:
                    frames.Add(Snapshot(objects));
                    break;

                default:
                    break;
            }
        }

        if (frames.Count is 0)
            frames.Add(Snapshot(objects));

        return new Timeline(Union(frames.Select(frame => frame.Bounds)), frames);
    }

    private void Apply(Dictionary<int, FrameObject> objects, PlaceObjectAction action, int depth, Matrix? matrix, ColorTransform? colorTransform, int? ratio, string? name, int? clipDepth, Filter[] filters)
    {
        switch (action)
        {
            case PlaceObjectAction.PlaceObjectActionPlace place:
                objects[depth] = NewObject(depth, place.Id, matrix, colorTransform, ratio, name, clipDepth, filters);
                break;

            case PlaceObjectAction.PlaceObjectActionReplace replace:
                objects[depth] = NewObject(depth, replace.Id, matrix ?? (objects.TryGetValue(depth, out var previous) ? previous.Matrix : null), colorTransform, ratio, name, clipDepth, filters);
                break;

            case PlaceObjectAction.PlaceObjectActionModify when objects.TryGetValue(depth, out var existing):
                objects[depth] = existing with
                {
                    Matrix = matrix ?? existing.Matrix,
                    ColorTransform = colorTransform ?? existing.ColorTransform,
                    Ratio = ratio ?? existing.Ratio,
                    Name = name ?? existing.Name,
                    ClipDepth = clipDepth ?? existing.ClipDepth,
                    Filters = filters.Length > 0 ? filters : existing.Filters
                };
                break;

            default:
                break;
        }
    }

    private FrameObject NewObject(int depth, int id, Matrix? matrix, ColorTransform? colorTransform, int? ratio, string? name, int? clipDepth, Filter[] filters)
    {
        return new FrameObject
        {
            Depth = depth,
            Drawable = _characters.Character(id),
            Matrix = matrix ?? Identity,
            ColorTransform = colorTransform,
            Ratio = ratio,
            Name = name,
            ClipDepth = clipDepth,
            Filters = filters
        };
    }

    private static Frame Snapshot(Dictionary<int, FrameObject> objects)
    {
        var ordered = objects.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();
        var bounds = Union(ordered.Where(item => item.ClipDepth is null).Select(item => TransformBounds(item.Matrix, item.Drawable.Bounds)));
        return new Frame(bounds, ordered);
    }

    private static Rectangle Union(IEnumerable<Rectangle> rectangles)
    {
        var hasAny = false;
        var xMin = int.MaxValue;
        var yMin = int.MaxValue;
        var xMax = int.MinValue;
        var yMax = int.MinValue;

        foreach (var rectangle in rectangles)
        {
            hasAny = true;
            xMin = Math.Min(xMin, rectangle.XMin);
            yMin = Math.Min(yMin, rectangle.YMin);
            xMax = Math.Max(xMax, rectangle.XMax);
            yMax = Math.Max(yMax, rectangle.YMax);
        }

        return hasAny ? new Rectangle(xMin, xMax, yMin, yMax) : new Rectangle(0, 0, 0, 0);
    }

    private static Rectangle TransformBounds(Matrix matrix, Rectangle rectangle)
    {
        var a = matrix.Scale.X.ToSingle();
        var b = matrix.Rotation.X.ToSingle();
        var c = matrix.Rotation.Y.ToSingle();
        var d = matrix.Scale.Y.ToSingle();
        float tx = matrix.Translation.X;
        float ty = matrix.Translation.Y;

        Span<int> xs = [rectangle.XMin, rectangle.XMax, rectangle.XMin, rectangle.XMax];
        Span<int> ys = [rectangle.YMin, rectangle.YMin, rectangle.YMax, rectangle.YMax];

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        for (var i = 0; i < 4; i++)
        {
            var px = (a * xs[i]) + (c * ys[i]) + tx;
            var py = (b * xs[i]) + (d * ys[i]) + ty;
            minX = Math.Min(minX, px);
            maxX = Math.Max(maxX, px);
            minY = Math.Min(minY, py);
            maxY = Math.Max(maxY, py);
        }

        return new Rectangle((int)Math.Floor(minX), (int)Math.Ceiling(maxX), (int)Math.Floor(minY), (int)Math.Ceiling(maxY));
    }
}
