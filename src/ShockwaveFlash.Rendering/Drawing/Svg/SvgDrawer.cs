using System.Globalization;
using System.Text;
using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Model.Sprites;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Filter;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Rendering.Drawing.Svg;

public sealed class SvgDrawer : IDrawer<string>
{
    private readonly StringBuilder _defs = new();

    private readonly StringBuilder _body = new();

    private Rectangle _bounds;

    private int _gradientId;

    private int _patternId;

    private int _clipId;

    private int _filterId;

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
            AppendStroke(style);
        }
        else if (style.LineColor is { } lineColor)
        {
            _body.Append(CultureInfo.InvariantCulture, $" stroke=\"{Hex(lineColor)}\"");

            if (lineColor.A < 255)
                _body.Append(CultureInfo.InvariantCulture, $" stroke-opacity=\"{Opacity(lineColor)}\"");

            AppendStroke(style);
        }

        _body.Append("/>");
    }

    public void Image(IImage image)
    {
        _body.Append(CultureInfo.InvariantCulture, $"<image width=\"{Px(image.Bounds.Width)}\" height=\"{Px(image.Bounds.Height)}\" href=\"{image.ToBase64Data()}\"/>");
    }

    public void Include(IDrawable drawable, Matrix matrix, int frame, IReadOnlyList<Filter> filters, BlendMode blendMode, string? name)
    {
        _body.Append(CultureInfo.InvariantCulture, $"<g transform=\"{Transform(matrix)}\"");

        if (BuildFilter(filters) is { } filterId)
            _body.Append(CultureInfo.InvariantCulture, $" filter=\"url(#{filterId})\"");

        if (blendMode.ToCssMixBlendMode() is { } blend)
            _body.Append(CultureInfo.InvariantCulture, $" style=\"mix-blend-mode:{blend}\"");

        _body.Append('>');
        drawable.Draw(this, frame);
        _body.Append("</g>");
    }

    private string? BuildFilter(IReadOnlyList<Filter> filters)
    {
        var primitives = new StringBuilder();
        var last = "SourceGraphic";
        var index = 0;

        foreach (var filter in filters)
        {
            var result = $"r{index++}";

            switch (filter)
            {
                case BlurFilter blur:
                    primitives.Append(CultureInfo.InvariantCulture, $"<feGaussianBlur in=\"{last}\" stdDeviation=\"{Sigma(blur.Blur.X, blur.Passes)} {Sigma(blur.Blur.Y, blur.Passes)}\" result=\"{result}\"/>");
                    break;

                case GlowFilter { IsInner: true } glow:
                    AppendInnerGlow(primitives, last, result, Sigma(glow.Blur.X, glow.Passes), glow.Color);
                    break;

                case GlowFilter glow:
                    AppendShadow(primitives, last, result, 0, 0, Sigma(glow.Blur.X, glow.Passes), glow.Color);
                    break;

                case DropShadowFilter { IsInner: true } shadow:
                    AppendInnerGlow(primitives, last, result, Sigma(shadow.Blur.X, shadow.Passes), shadow.Color);
                    break;

                case DropShadowFilter shadow:
                    var angle = shadow.Angle.ToSingle();
                    var distance = shadow.Distance.ToSingle();
                    AppendShadow(primitives, last, result, distance * Math.Cos(angle), distance * Math.Sin(angle), Sigma(shadow.Blur.X, shadow.Passes), shadow.Color);
                    break;

                case ColorMatrixFilter matrix when !matrix.Impotent:
                    AppendColorMatrix(primitives, last, result, matrix.Matrix);
                    break;

                default:
                    continue;
            }

            last = result;
        }

        if (primitives.Length is 0)
            return null;

        var id = $"flt{_filterId++}";
        _defs.Append(CultureInfo.InvariantCulture, $"<filter id=\"{id}\" x=\"-50%\" y=\"-50%\" width=\"200%\" height=\"200%\">{primitives}</filter>");
        return id;
    }

    private static void AppendShadow(StringBuilder primitives, string input, string result, double dx, double dy, float deviation, Color color)
    {
        primitives.Append(CultureInfo.InvariantCulture, $"<feDropShadow in=\"{input}\" dx=\"{dx}\" dy=\"{dy}\" stdDeviation=\"{deviation}\" flood-color=\"{Hex(color)}\" flood-opacity=\"{Opacity(color)}\" result=\"{result}\"/>");
    }

    private static void AppendInnerGlow(StringBuilder primitives, string input, string result, float deviation, Color color)
    {
        // Inner glow: invert the source alpha, blur it, clip to the source, flood with the glow colour
        // and composite the rim back over the source (mirrors the Skia inner-shadow path).
        primitives.Append(CultureInfo.InvariantCulture, $"<feComponentTransfer in=\"{input}\" result=\"{result}_i\"><feFuncA type=\"table\" tableValues=\"1 0\"/></feComponentTransfer>");
        primitives.Append(CultureInfo.InvariantCulture, $"<feGaussianBlur in=\"{result}_i\" stdDeviation=\"{deviation}\" result=\"{result}_b\"/>");
        primitives.Append(CultureInfo.InvariantCulture, $"<feFlood flood-color=\"{Hex(color)}\" flood-opacity=\"{Opacity(color)}\" result=\"{result}_f\"/>");
        primitives.Append(CultureInfo.InvariantCulture, $"<feComposite in=\"{result}_f\" in2=\"{result}_b\" operator=\"in\" result=\"{result}_r\"/>");
        primitives.Append(CultureInfo.InvariantCulture, $"<feComposite in=\"{result}_r\" in2=\"{input}\" operator=\"in\" result=\"{result}_c\"/>");
        primitives.Append(CultureInfo.InvariantCulture, $"<feMerge result=\"{result}\"><feMergeNode in=\"{input}\"/><feMergeNode in=\"{result}_c\"/></feMerge>");
    }

    private static void AppendColorMatrix(StringBuilder primitives, string input, string result, float[] matrix)
    {
        var values = new StringBuilder();

        for (var i = 0; i < matrix.Length; i++)
        {
            if (i > 0)
                values.Append(' ');

            var value = i % 5 is 4 ? matrix[i] / 255f : matrix[i];
            values.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        primitives.Append(CultureInfo.InvariantCulture, $"<feColorMatrix in=\"{input}\" type=\"matrix\" values=\"{values}\" result=\"{result}\"/>");
    }

    private static float Sigma(Fixed16 blur, int passes)
    {
        var width = blur.ToSingle();
        return width <= 1f || passes <= 0 ? 0f : width * 0.2886751f * MathF.Sqrt(passes);
    }

    public string StartClip(IDrawable drawable, Matrix matrix, int frame)
    {
        var id = $"c{_clipId++}";

        _defs.Append(CultureInfo.InvariantCulture, $"<clipPath id=\"{id}\">");

        AppendClip(
            drawable,
            matrix.Scale.X.ToSingle(),
            matrix.Rotation.X.ToSingle(),
            matrix.Rotation.Y.ToSingle(),
            matrix.Scale.Y.ToSingle(),
            matrix.Translation.X / 20.0,
            matrix.Translation.Y / 20.0,
            0);

        _defs.Append("</clipPath>");
        _body.Append(CultureInfo.InvariantCulture, $"<g clip-path=\"url(#{id})\">");

        return id;
    }

    private void AppendClip(IDrawable drawable, float a, float b, float c, float d, double e, double f, int depth)
    {
        if (depth > 8)
            return;

        switch (drawable)
        {
            case ShapeDefinition definition:
                foreach (var path in definition.Shape.Paths)
                    if (path.Style.Fill is not null)
                        _defs.Append(CultureInfo.InvariantCulture, $"<path d=\"{BuildData(path.Edges)}\" transform=\"matrix({a} {b} {c} {d} {e} {f})\"/>");

                break;

            case SpriteDefinition sprite:
                var frames = sprite.Timeline.Frames;

                if (frames.Count is 0)
                    break;

                foreach (var item in frames[0].Objects)
                {
                    if (item.ClipDepth is not null)
                        continue;

                    var m = item.Matrix;
                    var ca = m.Scale.X.ToSingle();
                    var cb = m.Rotation.X.ToSingle();
                    var cc = m.Rotation.Y.ToSingle();
                    var cd = m.Scale.Y.ToSingle();
                    var ce = m.Translation.X / 20.0;
                    var cf = m.Translation.Y / 20.0;

                    AppendClip(
                        item.Drawable,
                        (a * ca) + (c * cb),
                        (b * ca) + (d * cb),
                        (a * cc) + (c * cd),
                        (b * cc) + (d * cd),
                        (a * ce) + (c * cf) + e,
                        (b * ce) + (d * cf) + f,
                        depth + 1);
                }

                break;

            default:
                break;
        }
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

    private void AppendStroke(PathStyle style)
    {
        _body.Append(CultureInfo.InvariantCulture, $" stroke-width=\"{Px(style.LineWidth)}\" stroke-linecap=\"{Cap(style.LineCap)}\" stroke-linejoin=\"{Join(style.LineJoin)}\"");

        if (style.LineJoin is LineJoinStyleMiter)
            _body.Append(CultureInfo.InvariantCulture, $" stroke-miterlimit=\"{Math.Max(style.MiterLimit, 1f).ToString(CultureInfo.InvariantCulture)}\"");
    }

    private static string Cap(LineCapStyle cap)
    {
        return cap switch
        {
            LineCapStyle.None => "butt",
            LineCapStyle.Square => "square",
            _ => "round"
        };
    }

    private static string Join(LineJoinStyle? join)
    {
        return join switch
        {
            LineJoinStyleBevel => "bevel",
            LineJoinStyleMiter => "miter",
            _ => "round"
        };
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

        _defs.Append(CultureInfo.InvariantCulture, $"<linearGradient id=\"{id}\" gradientUnits=\"userSpaceOnUse\" gradientTransform=\"{Transform(fill.Matrix)}\" spreadMethod=\"{Spread(fill.Gradient.Spread)}\" x1=\"-819.2\" x2=\"819.2\">");
        AppendStops(fill.Gradient);
        _defs.Append("</linearGradient>");

        return id;
    }

    private string RadialGradient(RadialGradientFill fill)
    {
        var id = $"g{_gradientId++}";

        _defs.Append(CultureInfo.InvariantCulture, $"<radialGradient id=\"{id}\" gradientUnits=\"userSpaceOnUse\" gradientTransform=\"{Transform(fill.Matrix)}\" spreadMethod=\"{Spread(fill.Gradient.Spread)}\" cx=\"0\" cy=\"0\" r=\"819.2\"");

        if (fill.Gradient.FocalPoint is { } focal)
            _defs.Append(CultureInfo.InvariantCulture, $" fx=\"{(focal * 819.2).ToString(CultureInfo.InvariantCulture)}\" fy=\"0\"");

        _defs.Append('>');
        AppendStops(fill.Gradient);
        _defs.Append("</radialGradient>");

        return id;
    }

    private void AppendStops(Gradient gradient)
    {
        // Flash interpolates LinearRGB gradients in linear light; SVG interpolates in sRGB, so bake a
        // dense ramp sampled in linear light for that mode (mirrors the Skia backend for parity).
        if (gradient.Interpolation is GradientInterpolation.LinearRgb && gradient.Stops.Count > 1)
        {
            const int samples = 64;

            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)(samples - 1);
                AppendStop(t, SampleLinear(gradient.Stops, t));
            }

            return;
        }

        foreach (var stop in gradient.Stops)
            AppendStop(stop.Ratio / 255.0, stop.Color);
    }

    private void AppendStop(double offset, Color color)
    {
        _defs.Append(CultureInfo.InvariantCulture, $"<stop offset=\"{offset.ToString(CultureInfo.InvariantCulture)}\" stop-color=\"{Hex(color)}\"");

        if (color.A < 255)
            _defs.Append(CultureInfo.InvariantCulture, $" stop-opacity=\"{Opacity(color)}\"");

        _defs.Append("/>");
    }

    private static Color SampleLinear(IReadOnlyList<GradientStop> stops, float t)
    {
        var lo = stops[0];
        var hi = stops[^1];

        for (var i = 0; i < stops.Count - 1; i++)
        {
            var a = stops[i].Ratio / 255f;
            var b = stops[i + 1].Ratio / 255f;

            if (t <= a)
                return stops[i].Color;

            if (t < b)
            {
                lo = stops[i];
                hi = stops[i + 1];
                break;
            }
        }

        if (t >= stops[^1].Ratio / 255f)
            return stops[^1].Color;

        var span = (hi.Ratio - lo.Ratio) / 255f;
        var k = span <= 0 ? 0f : (t - (lo.Ratio / 255f)) / span;

        return new Color(
            LerpLinear(lo.Color.R, hi.Color.R, k),
            LerpLinear(lo.Color.G, hi.Color.G, k),
            LerpLinear(lo.Color.B, hi.Color.B, k),
            (byte)Math.Clamp((int)Math.Round(lo.Color.A + ((hi.Color.A - lo.Color.A) * k)), 0, 255));
    }

    private static byte LerpLinear(byte a, byte b, float k)
    {
        var linear = SrgbToLinear(a / 255f) + ((SrgbToLinear(b / 255f) - SrgbToLinear(a / 255f)) * k);
        return (byte)Math.Clamp((int)Math.Round(LinearToSrgb(linear) * 255f), 0, 255);
    }

    private static float SrgbToLinear(float c)
    {
        return c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    private static float LinearToSrgb(float c)
    {
        return c <= 0.0031308f ? c * 12.92f : (1.055f * MathF.Pow(c, 1f / 2.4f)) - 0.055f;
    }

    private static string Spread(GradientSpread spread)
    {
        return spread switch
        {
            GradientSpread.Reflect => "reflect",
            GradientSpread.Repeat => "repeat",
            _ => "pad"
        };
    }

    private static string BuildData(IReadOnlyList<IEdge> edges)
    {
        var builder = new StringBuilder();
        var startX = int.MinValue;
        var startY = int.MinValue;
        var lastX = int.MinValue;
        var lastY = int.MinValue;
        var open = false;

        foreach (var edge in edges)
        {
            if (edge.FromX != lastX || edge.FromY != lastY)
            {
                if (open && startX == lastX && startY == lastY)
                    builder.Append('Z');

                builder.Append(CultureInfo.InvariantCulture, $"M{Px(edge.FromX)} {Px(edge.FromY)}");
                startX = edge.FromX;
                startY = edge.FromY;
                open = true;
            }

            if (edge is CurvedEdge curve)
                builder.Append(CultureInfo.InvariantCulture, $"Q{Px(curve.ControlX)} {Px(curve.ControlY)} {Px(curve.ToX)} {Px(curve.ToY)}");
            else
                builder.Append(CultureInfo.InvariantCulture, $"L{Px(edge.ToX)} {Px(edge.ToY)}");

            lastX = edge.ToX;
            lastY = edge.ToY;
        }

        if (open && startX == lastX && startY == lastY)
            builder.Append('Z');

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
