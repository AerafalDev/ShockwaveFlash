using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Rendering.Processing;
using ShockwaveFlash.Tags.Shape;
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Rendering.Model.Shapes;

public sealed class ShapeDefinition : IDrawable
{
    private readonly Rectangle _bounds;

    private Shape? _shape;

    private ShapeProcessor? _processor;

    private DefineShapeTag? _tag;

    public Rectangle Bounds => _bounds;

    public Shape Shape => _shape ??= Materialize();

    public ShapeDefinition(ShapeProcessor processor, DefineShapeTag tag)
    {
        _processor = processor;
        _tag = tag;
        _bounds = tag.ShapeBounds;
    }

    private ShapeDefinition(Shape shape, Rectangle bounds)
    {
        _shape = shape;
        _bounds = bounds;
    }

    public int FrameCount(bool recursive = false)
    {
        return 1;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Shape(Shape);
        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return new ShapeDefinition(Shape.TransformColors(colorTransform), _bounds);
    }

    private Shape Materialize()
    {
        var shape = _processor!.Process(_tag!);
        _processor = null;
        _tag = null;
        return shape;
    }
}
