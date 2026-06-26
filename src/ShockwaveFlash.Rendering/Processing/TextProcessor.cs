using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Model.Text;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Rendering.Processing;

public sealed class TextProcessor
{
    private readonly IFontResolver _fonts;

    private readonly ShapeProcessor _shapes;

    public TextProcessor(IFontResolver fonts, ShapeProcessor shapes)
    {
        _fonts = fonts;
        _shapes = shapes;
    }

    public TextDefinition Process(Rectangle bounds, Matrix matrix, IReadOnlyList<TextRecord> records)
    {
        var glyphs = new List<TextGlyph>();

        ResolvedFont? font = null;
        var color = new Color(0, 0, 0, 255);
        var height = 0;
        var penX = 0;
        var penY = 0;

        foreach (var record in records)
        {
            if (record.Id is { } fontId)
                font = _fonts.ResolveFont(fontId);

            if (record.Color is { } recordColor)
                color = recordColor;

            if (record.Height is { } recordHeight)
                height = recordHeight;

            if (record.OffsetX is { } offsetX)
                penX = offsetX;

            if (record.OffsetY is { } offsetY)
                penY = offsetY;

            var scale = font is null ? 0f : height / font.EmSquare;

            foreach (var entry in record.Glyphs)
            {
                if (font is not null && entry.Index < font.Glyphs.Count)
                {
                    var glyph = font.Glyphs[(int)entry.Index];

                    if (glyph.Shapes.Count > 0)
                    {
                        var shape = _shapes.ProcessGlyph(glyph.Shapes, color);
                        var drawable = ShapeDefinition.FromShape(shape, bounds);
                        glyphs.Add(new TextGlyph(drawable, Compose(matrix, scale, penX, penY)));
                    }
                }

                penX += entry.Advance;
            }
        }

        return new TextDefinition(bounds, glyphs);
    }

    private static Matrix Compose(Matrix text, float scale, int penX, int penY)
    {
        var a = text.Scale.X.ToSingle();
        var b = text.Rotation.X.ToSingle();
        var c = text.Rotation.Y.ToSingle();
        var d = text.Scale.Y.ToSingle();
        var e = text.Translation.X;
        var f = text.Translation.Y;

        var scaleComponent = new FixedPoint2(Fixed16.FromSingle(a * scale), Fixed16.FromSingle(d * scale));
        var rotationComponent = new FixedPoint2(Fixed16.FromSingle(b * scale), Fixed16.FromSingle(c * scale));
        var translation = new Point(
            (int)Math.Round(a * penX + c * penY + e),
            (int)Math.Round(b * penX + d * penY + f));

        return new Matrix(scaleComponent, rotationComponent, translation);
    }
}
