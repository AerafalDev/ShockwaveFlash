using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Processing;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model;

public sealed class MovieDefinition : IDrawable
{
    private readonly TimelineProcessor _processor;

    private readonly IReadOnlyList<Tag> _tags;

    private readonly Rectangle _stage;

    private readonly Color? _background;

    private Timeline? _timeline;

    public Rectangle Bounds => _stage;

    public Color? BackgroundColor => _background;

    private Timeline Timeline => _timeline ??= _processor.Process(_tags);

    public MovieDefinition(TimelineProcessor processor, IReadOnlyList<Tag> tags, Rectangle stage, Color? background)
    {
        _processor = processor;
        _tags = tags;
        _stage = stage;
        _background = background;
    }

    public int FrameCount(bool recursive = false)
    {
        return Timeline.FrameCount(recursive);
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        if (_background is { } color)
            drawer.Shape(Background(color, _stage));

        return Timeline.Draw(drawer, frame);
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return Timeline.TransformColors(colorTransform);
    }

    private static Shape Background(Color color, Rectangle stage)
    {
        var style = new PathStyle { Fill = new SolidFill(color) };

        IEdge[] edges =
        [
            new StraightEdge(stage.XMin, stage.YMin, stage.XMax, stage.YMin),
            new StraightEdge(stage.XMax, stage.YMin, stage.XMax, stage.YMax),
            new StraightEdge(stage.XMax, stage.YMax, stage.XMin, stage.YMax),
            new StraightEdge(stage.XMin, stage.YMax, stage.XMin, stage.YMin),
        ];

        return new Shape(stage.Width, stage.Height, -stage.XMin, -stage.YMin, [new ShapePath(edges, style)]);
    }
}
