using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Button;

namespace ShockwaveFlash.Rendering.Model.Buttons;

public sealed class ButtonDefinition : IDrawable
{
    private readonly ICharacterResolver _characters;

    private readonly IReadOnlyList<ButtonRecord> _records;

    private Frame? _frame;

    public Rectangle Bounds => Frame.Bounds;

    private Frame Frame => _frame ??= Build();

    public ButtonDefinition(ICharacterResolver characters, IReadOnlyList<ButtonRecord> records)
    {
        _characters = characters;
        _records = records;
    }

    public int FrameCount(bool recursive = false)
    {
        return 1;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        Frame.Draw(drawer, frame);
        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return new ButtonDefinition(_characters, [.. _records.Select(record => record with { ColorTransform = colorTransform.Merge(record.ColorTransform) })]);
    }

    private Frame Build()
    {
        var objects = new List<FrameObject>();

        foreach (var record in _records.Where(record => record.States.HasFlag(ButtonStates.Up)).OrderBy(record => record.Depth))
        {
            objects.Add(new FrameObject
            {
                Depth = record.Depth,
                Drawable = _characters.Character(record.Id),
                Matrix = record.Matrix,
                ColorTransform = record.ColorTransform,
                Filters = record.Filters,
                BlendMode = MapBlend(record.BlendMode)
            });
        }

        var bounds = Union(objects.Select(item => TransformBounds(item.Matrix, item.Drawable.Bounds)));
        return new Frame(bounds, objects);
    }

    private static BlendMode MapBlend(ShockwaveFlash.Types.DisplayList.BlendMode blend)
    {
        return (int)blend is 0 ? BlendMode.Normal : (BlendMode)(int)blend;
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
