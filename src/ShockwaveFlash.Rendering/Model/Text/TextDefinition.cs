using ShockwaveFlash.Rendering.Drawing;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Filter;

namespace ShockwaveFlash.Rendering.Model.Text;

public sealed record TextGlyph(IDrawable Drawable, Matrix Matrix);

public sealed class TextDefinition : IDrawable
{
    private static readonly IReadOnlyList<Filter> s_noFilters = [];

    private readonly Rectangle _bounds;

    private readonly IReadOnlyList<TextGlyph> _glyphs;

    public Rectangle Bounds => _bounds;

    public TextDefinition(Rectangle bounds, IReadOnlyList<TextGlyph> glyphs)
    {
        _bounds = bounds;
        _glyphs = glyphs;
    }

    public int FrameCount(bool recursive = false)
    {
        return 1;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        foreach (var glyph in _glyphs)
            drawer.Include(glyph.Drawable, glyph.Matrix, frame, s_noFilters, BlendMode.Normal, null);

        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return new TextDefinition(_bounds, [.. _glyphs.Select(glyph => glyph with { Drawable = glyph.Drawable.TransformColors(colorTransform) })]);
    }
}
