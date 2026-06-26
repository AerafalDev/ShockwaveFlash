using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Rendering.Processing;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Tags.Sprite;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Sprites;

public sealed class SpriteDefinition : IDrawable
{
    private readonly TimelineProcessor _processor;

    private readonly DefineSpriteTag _tag;

    private Timeline? _timeline;

    private bool _processing;

    public Timeline Timeline => _timeline ??= Build();

    public Rectangle Bounds => Timeline.Bounds;

    public SpriteDefinition(TimelineProcessor processor, DefineSpriteTag tag)
    {
        _processor = processor;
        _tag = tag;
    }

    public int FrameCount(bool recursive = false)
    {
        return Timeline.FrameCount(recursive);
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        return Timeline.Draw(drawer, frame);
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return Timeline.TransformColors(colorTransform);
    }

    private Timeline Build()
    {
        if (_processing)
            return new Timeline(new Rectangle(0, 0, 0, 0), []);

        _processing = true;

        try
        {
            return _processor.Process(_tag.Tags);
        }
        finally
        {
            _processing = false;
        }
    }
}
