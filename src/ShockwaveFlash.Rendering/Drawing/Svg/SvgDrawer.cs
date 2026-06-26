using System.Globalization;
using System.Text;
using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Filter;

namespace ShockwaveFlash.Rendering.Drawing.Svg;

public sealed class SvgDrawer : IDrawer<string>
{
    private readonly StringBuilder _defs = new();

    private readonly StringBuilder _body = new();

    private Rectangle _bounds;

    private int _gradientId;

    private int _patternId;

    private int _clipId;

    public SvgDrawer(Rectangle bounds)
    {
        _bounds = bounds;
    }

    public static string RenderToSvg(IDrawable drawable, int frame = 0)
    {
        var drawer = new SvgDrawer(drawable.Bounds);
        drawable.Draw(drawer, frame);
        return drawer.Render();
    }

    public void Area(Rectangle bounds)
    {
    }

    public void Shape(Shape shape)
    {
        foreach (var path in shape.Paths)
            Path(path);
    }

    public void Path(ShapePath path)
    {
        var style = path.Style;

        if (style.IsEmpty)
            return;

        _body.Append("<path d=\"").Append(BuildData(path.Edges)).Append('"');

        if (style.Fill is null)
        {
            _body.Append(" fill=\"none\"");
        }
        else
        {
            _body.Append(" fill-rule=\"evenodd\"");
            AppendPaint(style.Fill, "fill");
        }

        if (style.LineFill is not null)
        {
            AppendPaint(style.LineFill, "stroke");
            AppendStrokeWidth(style.LineWidth);
        }
        else if (style.LineColor is { } lineColor)
        {
            _body.Append(CultureInfo.InvariantCulture, $" stroke=\"{Hex(lineColor)}\"");

            if (lineColor.A < 255)
                _body.Append(CultureInfo.InvariantCulture, $" stroke-opacity=\"{Opacity(lineColor)}\"");

            AppendStrokeWidth(style.LineWidth);
        }

        _body.Append("/>");
    }

    public void Image(IImage image)
    {
        _body.Append(CultureInfo.InvariantCulture, $"<image width=\"{Px(image.Bounds.Width)}\" height=\"{Px(image.Bounds.Height)}\" href=\"{image.ToBase64Data()}\"/>");
    }

    public void Include(IDrawable drawable, Matrix matrix, int frame, IReadOnlyList<Filter> filters, BlendMode blendMode, string? name)
    {
        _body.Append(CultureInfo.InvariantCulture, $"<g transform=\"{Transform(matrix)}\">");
        drawable.Draw(this, frame);
        _body.Append("</g>");
    }

    public string StartClip(IDrawable drawable, Matrix matrix, int frame)
    {
        var id = $"c{_clipId++}";

        _defs.Append(CultureInfo.InvariantCulture, $"<clipPath id=\"{id}\">");

        if (drawable is ShapeDefinition definition)
            foreach (var path in definition.Shape.Paths)
                if (path.Style.Fill is not null)
                    _defs.Append(CultureInfo.InvariantCulture, $"<path d=\"{BuildData(path.Edges)}\" transform=\"{Transform(matrix)}\"/>");

        _defs.Append("</clipPath>");
        _body.Append(CultureInfo.InvariantCulture, $"<g clip-path=\"url(#{id})\">");

        return id;
    }

    public void EndClip(string clipId)
    {
        _body.Append("</g>");
    }

    public string Render()
    {
        return new StringBuilder()
            .Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{Px(_bounds.Width)}\" height=\"{Px(_bounds.Height)}\" viewBox=\"{Px(_bounds.XMin)} {Px(_bounds.YMin)} {Px(_bounds.Width)} {Px(_bounds.Height)}\">")
            .Append("<defs>").Append(_defs).Append("</defs>")
            .Append(_body)
            .Append("</svg>")
            .ToString();
    }

    private void AppendPaint(IFillStyle fill, string attribute)
    {
        switch (fill)
        {
            case SolidFill solid:
                _body.Append(CultureInfo.InvariantCulture, $" {attribute}=\"{Hex(solid.Color)}\"");
                if (solid.Color.A < 255)
                    _body.Append(CultureInfo.InvariantCulture, $" {attribute}-opacity=\"{Opacity(solid.Color)}\"");
                break;

            case LinearGradientFill linear:
                _body.Append(CultureInfo.InvariantCulture, $" {attribute}=\"url(#{LinearGradient(linear)})\"");
                break;

            case RadialGradientFill radial:
                _body.Append(CultureInfo.InvariantCulture, $" {attribute}=\"url(#{RadialGradient(radial)})\"");
                break;

            case BitmapFill bitmap:
                _body.Append(CultureInfo.InvariantCulture, $" {attribute}=\"url(#{BitmapPattern(bitmap)})\"");
                break;

            default:
                _body.Append(CultureInfo.InvariantCulture, $" {attribute}=\"none\"");
                break;
        }
    }

    private void AppendStrokeWidth(int lineWidth)
    {
        _body.Append(CultureInfo.InvariantCulture, $" stroke-width=\"{Px(lineWidth)}\" stroke-linecap=\"round\" stroke-linejoin=\"round\"");
    }

    private string BitmapPattern(BitmapFill fill)
    {
        var id = $"p{_patternId++}";
        var width = (fill.Bitmap.Bounds.Width / 20.0).ToString(CultureInfo.InvariantCulture);
        var height = (fill.Bitmap.Bounds.Height / 20.0).ToString(CultureInfo.InvariantCulture);

        _defs.Append(CultureInfo.InvariantCulture, $"<pattern id=\"{id}\" patternUnits=\"userSpaceOnUse\" width=\"{width}\" height=\"{height}\" patternTransform=\"{BitmapTransform(fill.Matrix)}\">");
        _defs.Append(CultureInfo.InvariantCulture, $"<image width=\"{width}\" height=\"{height}\" href=\"{fill.Bitmap.ToBase64Data()}\"/>");
        _defs.Append("</pattern>");

        return id;
    }

    private static string BitmapTransform(Matrix matrix)
    {
        var a = matrix.Scale.X.ToSingle() / 20.0;
        var b = matrix.Rotation.X.ToSingle() / 20.0;
        var c = matrix.Rotation.Y.ToSingle() / 20.0;
        var d = matrix.Scale.Y.ToSingle() / 20.0;
        var e = matrix.Translation.X / 20.0;
        var f = matrix.Translation.Y / 20.0;

        return FormattableString.Invariant($"matrix({a} {b} {c} {d} {e} {f})");
    }

    private string LinearGradient(LinearGradientFill fill)
    {
        var id = $"g{_gradientId++}";

        _defs.Append(CultureInfo.InvariantCulture, $"<linearGradient id=\"{id}\" gradientUnits=\"userSpaceOnUse\" gradientTransform=\"{Transform(fill.Matrix)}\" x1=\"-819.2\" x2=\"819.2\">");
        AppendStops(fill.Gradient);
        _defs.Append("</linearGradient>");

        return id;
    }

    private string RadialGradient(RadialGradientFill fill)
    {
        var id = $"g{_gradientId++}";

        _defs.Append(CultureInfo.InvariantCulture, $"<radialGradient id=\"{id}\" gradientUnits=\"userSpaceOnUse\" gradientTransform=\"{Transform(fill.Matrix)}\" cx=\"0\" cy=\"0\" r=\"819.2\"");

        if (fill.Gradient.FocalPoint is { } focal)
            _defs.Append(CultureInfo.InvariantCulture, $" fx=\"0\" fy=\"{(focal * 819.2).ToString(CultureInfo.InvariantCulture)}\"");

        _defs.Append('>');
        AppendStops(fill.Gradient);
        _defs.Append("</radialGradient>");

        return id;
    }

    private void AppendStops(Gradient gradient)
    {
        foreach (var stop in gradient.Stops)
        {
            _defs.Append(CultureInfo.InvariantCulture, $"<stop offset=\"{(stop.Ratio / 255.0).ToString(CultureInfo.InvariantCulture)}\" stop-color=\"{Hex(stop.Color)}\"");

            if (stop.Color.A < 255)
                _defs.Append(CultureInfo.InvariantCulture, $" stop-opacity=\"{Opacity(stop.Color)}\"");

            _defs.Append("/>");
        }
    }

    private static string BuildData(IReadOnlyList<IEdge> edges)
    {
        var builder = new StringBuilder();
        var lastX = int.MinValue;
        var lastY = int.MinValue;

        foreach (var edge in edges)
        {
            if (edge.FromX != lastX || edge.FromY != lastY)
                builder.Append(CultureInfo.InvariantCulture, $"M{Px(edge.FromX)} {Px(edge.FromY)}");

            if (edge is CurvedEdge curve)
                builder.Append(CultureInfo.InvariantCulture, $"Q{Px(curve.ControlX)} {Px(curve.ControlY)} {Px(curve.ToX)} {Px(curve.ToY)}");
            else
                builder.Append(CultureInfo.InvariantCulture, $"L{Px(edge.ToX)} {Px(edge.ToY)}");

            lastX = edge.ToX;
            lastY = edge.ToY;
        }

        return builder.ToString();
    }

    private static string Transform(Matrix matrix)
    {
        var a = matrix.Scale.X.ToSingle();
        var b = matrix.Rotation.X.ToSingle();
        var c = matrix.Rotation.Y.ToSingle();
        var d = matrix.Scale.Y.ToSingle();
        var e = matrix.Translation.X / 20.0;
        var f = matrix.Translation.Y / 20.0;

        return FormattableString.Invariant($"matrix({a} {b} {c} {d} {e} {f})");
    }

    private static string Hex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static string Opacity(Color color)
    {
        return (color.A / 255.0).ToString(CultureInfo.InvariantCulture);
    }

    private static string Px(int twips)
    {
        return Math.Round(twips / 20.0, 2).ToString(CultureInfo.InvariantCulture);
    }
}
