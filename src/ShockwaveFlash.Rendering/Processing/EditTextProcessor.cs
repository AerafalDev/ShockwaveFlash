using System.Globalization;
using System.Text;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Model.Text;
using ShockwaveFlash.Tags.Text;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Rendering.Processing;

public sealed class EditTextProcessor
{
    private const int Gutter = 40;

    private readonly IFontResolver _fonts;

    private readonly ShapeProcessor _shapes;

    public EditTextProcessor(IFontResolver fonts, ShapeProcessor shapes)
    {
        _fonts = fonts;
        _shapes = shapes;
    }

    public TextDefinition Process(DefineEditTextTag tag)
    {
        var bounds = tag.Bounds;

        if (!tag.HasText || string.IsNullOrEmpty(tag.InitialText))
            return new TextDefinition(bounds, []);

        var font = tag.FontId is { } fontId ? _fonts.ResolveFont(fontId) : null;

        if (font is null || font.Glyphs.Count is 0)
            return new TextDefinition(bounds, []);

        var height = tag.Height is { } h ? h : 240;
        var color = tag.Color ?? new Color(0, 0, 0, 255);
        var layout = tag.Layout;
        var defaultAlign = layout?.Alignment ?? TextAlignment.Left;
        var leftMargin = layout?.LeftMargin ?? 0;
        var rightMargin = layout?.RightMargin ?? 0;
        var indent = layout?.Indent ?? 0;
        var leading = layout?.Leading ?? 0;

        var windowWidth = Math.Max(0, bounds.Width - (Gutter * 2));
        var wrap = tag.WordWrap;

        var paragraphs = tag.Html
            ? ParseHtml(tag.InitialText, defaultAlign, color, height, font)
            : ParsePlain(tag.InitialText, defaultAlign, color, height, font);

        var glyphs = new List<TextGlyph>();
        var originX = bounds.XMin + Gutter;
        var originY = bounds.YMin + Gutter;
        var top = 0;
        var lineIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            var firstLineOfPara = true;

            foreach (var line in WrapLines(paragraph.Entries, windowWidth - leftMargin - rightMargin, wrap))
            {
                var maxHeight = height;
                var lineWidth = 0;

                foreach (var entry in line)
                {
                    if (!entry.IsBreak)
                    {
                        maxHeight = Math.Max(maxHeight, entry.Height);
                        lineWidth += entry.Advance;
                    }
                }

                var ascent = (int)(font.Ascent * (maxHeight / font.EmSquare));
                var descent = (int)(font.Descent * (maxHeight / font.EmSquare));

                var leftAdjust = leftMargin + (firstLineOfPara ? indent : 0);
                var misalign = windowWidth - leftAdjust - rightMargin - lineWidth;
                var alignOffset = paragraph.Align switch
                {
                    TextAlignment.Center => misalign / 2,
                    TextAlignment.Right => misalign,
                    _ => 0
                };

                var penX = originX + leftAdjust + Math.Max(0, alignOffset);
                var baselineY = originY + top + ascent;

                foreach (var entry in line)
                {
                    if (entry.IsBreak)
                        continue;

                    if (font.Glyphs[entry.GlyphIndex].Shapes.Count > 0)
                    {
                        var shape = _shapes.ProcessGlyph(font.Glyphs[entry.GlyphIndex].Shapes, entry.Color);
                        var drawable = ShapeDefinition.FromShape(shape, bounds);
                        glyphs.Add(new TextGlyph(drawable, Compose(entry.Scale, penX, baselineY)));
                    }

                    penX += entry.Advance;
                }

                top += ascent + descent + leading;
                lineIndex++;
                firstLineOfPara = false;
            }
        }

        return new TextDefinition(bounds, glyphs);
    }

    private static Matrix Compose(float scale, int penX, int penY)
    {
        return new Matrix(
            new FixedPoint2(Fixed16.FromSingle(scale), Fixed16.FromSingle(scale)),
            new FixedPoint2(Fixed16.Zero, Fixed16.Zero),
            new Point(penX, penY));
    }

    private static IEnumerable<List<Entry>> WrapLines(List<Entry> entries, int maxWidth, bool wrap)
    {
        var line = new List<Entry>();
        var width = 0;
        var lastSpace = -1;
        var widthAtSpace = 0;

        foreach (var entry in entries)
        {
            if (entry.IsBreak)
            {
                yield return line;
                line = new List<Entry>();
                width = 0;
                lastSpace = -1;
                continue;
            }

            line.Add(entry);
            width += entry.Advance;

            if (entry.IsSpace)
            {
                lastSpace = line.Count;
                widthAtSpace = width;
            }
            else if (wrap && maxWidth > 0 && width > maxWidth && lastSpace > 0 && lastSpace < line.Count)
            {
                var rest = line.GetRange(lastSpace, line.Count - lastSpace);
                line.RemoveRange(lastSpace, line.Count - lastSpace);
                yield return line;
                line = rest;
                width -= widthAtSpace;
                lastSpace = -1;
            }
        }

        yield return line;
    }

    private static List<Paragraph> ParsePlain(string text, TextAlignment align, Color color, int height, ResolvedFont font)
    {
        var entries = new List<Entry>();
        var scale = height / font.EmSquare;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c is '\r')
            {
                if (i + 1 < text.Length && text[i + 1] is '\n')
                    i++;

                entries.Add(Entry.Break);
                continue;
            }

            if (c is '\n')
            {
                entries.Add(Entry.Break);
                continue;
            }

            AppendChar(entries, font, c, color, scale, height);
        }

        return [new Paragraph(align, entries)];
    }

    private static List<Paragraph> ParseHtml(string html, TextAlignment defaultAlign, Color defaultColor, int defaultHeight, ResolvedFont font)
    {
        var paragraphs = new List<Paragraph>();
        var entries = new List<Entry>();
        var align = defaultAlign;
        var color = defaultColor;
        var height = defaultHeight;
        var started = false;

        void Flush()
        {
            paragraphs.Add(new Paragraph(align, entries));
            entries = new List<Entry>();
        }

        for (var i = 0; i < html.Length;)
        {
            var c = html[i];

            if (c is '<')
            {
                var close = html.IndexOf('>', i);
                if (close < 0)
                    break;

                var tag = html.Substring(i + 1, close - i - 1).Trim();
                i = close + 1;

                var name = TagName(tag);

                switch (name)
                {
                    case "br":
                        entries.Add(Entry.Break);
                        break;

                    case "p":
                        if (started)
                            Flush();

                        started = true;
                        align = ParseAlign(tag) ?? defaultAlign;
                        break;

                    case "font":
                        color = ParseColor(tag) ?? color;
                        height = ParseSize(tag) is { } size ? size * 20 : height;
                        break;

                    case "/font":
                        color = defaultColor;
                        height = defaultHeight;
                        break;

                    default:
                        break;
                }

                continue;
            }

            if (c is '&')
            {
                var semicolon = html.IndexOf(';', i);
                if (semicolon > i && semicolon - i <= 10)
                {
                    AppendChar(entries, font, DecodeEntity(html.Substring(i + 1, semicolon - i - 1)), color, height / font.EmSquare, height);
                    i = semicolon + 1;
                    continue;
                }
            }

            started = true;
            AppendChar(entries, font, c, color, height / font.EmSquare, height);
            i++;
        }

        Flush();
        return paragraphs;
    }

    private static void AppendChar(List<Entry> entries, ResolvedFont font, char c, Color color, float scale, int height)
    {
        if (c is '￿' or '\0')
            return;

        var index = font.IndexForCode(c);

        if (index < 0)
            return;

        var advance = (int)Math.Round(font.Glyphs[index].Advance * scale);
        entries.Add(new Entry(index, advance, color, scale, height, false, c is ' '));
    }

    private static string TagName(string tag)
    {
        var end = 0;
        var slash = tag.StartsWith('/') ? "/" : string.Empty;
        var body = slash.Length > 0 ? tag[1..] : tag;

        while (end < body.Length && !char.IsWhiteSpace(body[end]) && body[end] is not '/')
            end++;

        return slash + body[..end].ToLowerInvariant();
    }

    private static TextAlignment? ParseAlign(string tag)
    {
        return Attribute(tag, "align")?.ToLowerInvariant() switch
        {
            "center" => TextAlignment.Center,
            "right" => TextAlignment.Right,
            "justify" => TextAlignment.Justify,
            "left" => TextAlignment.Left,
            _ => null
        };
    }

    private static Color? ParseColor(string tag)
    {
        var value = Attribute(tag, "color");

        if (value is null || !value.StartsWith('#') || value.Length < 7)
            return null;

        if (int.TryParse(value.AsSpan(1, 6), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
            return new Color((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF), 255);

        return null;
    }

    private static int? ParseSize(string tag)
    {
        return int.TryParse(Attribute(tag, "size"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var size) ? size : null;
    }

    private static string? Attribute(string tag, string name)
    {
        var index = tag.IndexOf(name + "=", StringComparison.OrdinalIgnoreCase);

        if (index < 0)
            return null;

        var start = index + name.Length + 1;

        if (start >= tag.Length)
            return null;

        var quote = tag[start];

        if (quote is '"' or '\'')
        {
            var end = tag.IndexOf(quote, start + 1);
            return end < 0 ? null : tag[(start + 1)..end];
        }

        var space = start;
        while (space < tag.Length && !char.IsWhiteSpace(tag[space]))
            space++;

        return tag[start..space];
    }

    private static char DecodeEntity(string entity)
    {
        switch (entity.ToLowerInvariant())
        {
            case "lt":
                return '<';
            case "gt":
                return '>';
            case "amp":
                return '&';
            case "quot":
                return '"';
            case "apos":
                return '\'';
            case "nbsp":
                return ' ';
        }

        if (entity.StartsWith('#'))
        {
            var numeric = entity[1] is 'x' or 'X'
                ? int.TryParse(entity.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex) ? hex : -1
                : int.TryParse(entity.AsSpan(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var dec) ? dec : -1;

            if (numeric is > 0 and <= char.MaxValue)
                return (char)numeric;
        }

        return '￿';
    }

    private readonly record struct Entry(int GlyphIndex, int Advance, Color Color, float Scale, int Height, bool IsBreak, bool IsSpace)
    {
        public static Entry Break => new(-1, 0, default, 0f, 0, true, false);
    }

    private readonly record struct Paragraph(TextAlignment Align, List<Entry> Entries);
}
