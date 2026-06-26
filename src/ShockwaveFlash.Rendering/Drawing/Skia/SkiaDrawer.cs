using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Filter;
using ShockwaveFlash.Types.Shape;
using SkiaSharp;

namespace ShockwaveFlash.Rendering.Drawing.Skia;

public sealed class SkiaDrawer : IDrawer<SKImage>, IDisposable
{
    private const float GradientSquare = 819.2f;

    private readonly SKBitmap _bitmap;

    private readonly SKCanvas _canvas;

    public SkiaDrawer(Rectangle bounds, float scale = 1f, SKColor? background = null)
    {
        var width = Math.Max(1, (int)Math.Ceiling(bounds.Width / 20.0 * scale));
        var height = Math.Max(1, (int)Math.Ceiling(bounds.Height / 20.0 * scale));

        _bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        _canvas = new SKCanvas(_bitmap);
        _canvas.Clear(background ?? SKColors.Transparent);
        _canvas.Scale(scale);
        _canvas.Translate(-bounds.XMin / 20f, -bounds.YMin / 20f);
    }

    public static byte[] RenderToPng(IDrawable drawable, float scale = 1f, SKColor? background = null)
    {
        using var drawer = new SkiaDrawer(drawable.Bounds, scale, background);
        drawable.Draw(drawer);

        using var image = drawer.Render();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
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

        if (style.Fill is { } fill)
        {
            using var fillPath = BuildPath(path.Edges, false);
            FillPath(fillPath, fill);
        }

        if (style.LineColor is { } lineColor)
        {
            using var strokePath = BuildPath(path.Edges, true);
            StrokePath(strokePath, ToSkColor(lineColor), style);
        }
    }

    public void Image(IImage image)
    {
        using var skImage = SKImage.FromEncodedData(image.ToPng().ToArray());

        if (skImage is not null)
            _canvas.DrawImage(skImage, 0, 0);
    }

    public void Include(IDrawable drawable, Matrix matrix, int frame, IReadOnlyList<Filter> filters, BlendMode blendMode, string? name)
    {
        var local = LocalMatrix(matrix);

        _canvas.Save();
        _canvas.Concat(ref local);

        var created = new List<SKImageFilter>();
        var imageFilter = BuildImageFilter(filters, created);
        var blend = MapBlend(blendMode);

        if (imageFilter is not null || blend != SKBlendMode.SrcOver)
        {
            using var layerPaint = new SKPaint { ImageFilter = imageFilter, BlendMode = blend };
            _canvas.SaveLayer(layerPaint);
            drawable.Draw(this, frame);
            _canvas.Restore();
        }
        else
        {
            drawable.Draw(this, frame);
        }

        foreach (var filter in created)
            filter.Dispose();

        _canvas.Restore();
    }

    private static SKBlendMode MapBlend(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Multiply => SKBlendMode.Multiply,
            BlendMode.Screen => SKBlendMode.Screen,
            BlendMode.Lighten => SKBlendMode.Lighten,
            BlendMode.Darken => SKBlendMode.Darken,
            BlendMode.Difference => SKBlendMode.Difference,
            BlendMode.Add => SKBlendMode.Plus,
            BlendMode.Overlay => SKBlendMode.Overlay,
            BlendMode.HardLight => SKBlendMode.HardLight,
            _ => SKBlendMode.SrcOver
        };
    }

    private static SKImageFilter? BuildImageFilter(IReadOnlyList<Filter> filters, List<SKImageFilter> created)
    {
        SKImageFilter? current = null;

        foreach (var filter in filters)
        {
            var next = filter switch
            {
                BlurFilter blur => SKImageFilter.CreateBlur(Sigma(blur.Blur.X), Sigma(blur.Blur.Y), current),
                GlowFilter glow => SKImageFilter.CreateDropShadow(0, 0, Sigma(glow.Blur.X), Sigma(glow.Blur.Y), ToSkColor(glow.Color), current),
                DropShadowFilter shadow => SKImageFilter.CreateDropShadow(
                    shadow.Distance.ToSingle() * (float)Math.Cos(shadow.Angle.ToSingle()),
                    shadow.Distance.ToSingle() * (float)Math.Sin(shadow.Angle.ToSingle()),
                    Sigma(shadow.Blur.X),
                    Sigma(shadow.Blur.Y),
                    ToSkColor(shadow.Color),
                    current),
                _ => null
            };

            if (next is null)
                continue;

            created.Add(next);
            current = next;
        }

        return current;
    }

    private static float Sigma(Fixed16 blur)
    {
        return Math.Max(0f, blur.ToSingle() * 0.5f);
    }

    public string StartClip(IDrawable drawable, Matrix matrix, int frame)
    {
        _canvas.Save();

        using var clip = BuildClipPath(drawable);

        if (clip is not null)
        {
            clip.Transform(LocalMatrix(matrix));
            _canvas.ClipPath(clip, SKClipOperation.Intersect, true);
        }

        return string.Empty;
    }

    public void EndClip(string clipId)
    {
        _canvas.Restore();
    }

    public SKImage Render()
    {
        return SKImage.FromBitmap(_bitmap);
    }

    public void Dispose()
    {
        _canvas.Dispose();
        _bitmap.Dispose();
    }

    private void FillPath(SKPath skPath, IFillStyle fill)
    {
        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        SKImage? image = null;
        SKShader? shader = null;

        try
        {
            switch (fill)
            {
                case SolidFill solid:
                    paint.Color = ToSkColor(solid.Color);
                    break;

                case LinearGradientFill linear:
                    shader = LinearShader(linear);
                    break;

                case RadialGradientFill radial:
                    shader = RadialShader(radial);
                    break;

                case BitmapFill bitmap:
                    image = SKImage.FromEncodedData(bitmap.Bitmap.ToPng().ToArray());
                    if (image is null)
                        return;
                    var tile = bitmap.Repeat ? SKShaderTileMode.Repeat : SKShaderTileMode.Clamp;
                    shader = image.ToShader(tile, tile, BitmapMatrix(bitmap.Matrix));
                    break;

                default:
                    return;
            }

            paint.Shader = shader;
            _canvas.DrawPath(skPath, paint);
        }
        finally
        {
            shader?.Dispose();
            image?.Dispose();
        }
    }

    private void StrokePath(SKPath skPath, SKColor color, PathStyle style)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = color,
            StrokeWidth = Math.Max(style.LineWidth / 20f, 0.05f),
            StrokeCap = MapCap(style.LineCap),
            StrokeJoin = MapJoin(style.LineJoin),
            StrokeMiter = Math.Max(style.MiterLimit, 1f)
        };

        _canvas.DrawPath(skPath, paint);
    }

    private static SKStrokeCap MapCap(LineCapStyle cap)
    {
        return cap switch
        {
            LineCapStyle.None => SKStrokeCap.Butt,
            LineCapStyle.Square => SKStrokeCap.Square,
            _ => SKStrokeCap.Round
        };
    }

    private static SKStrokeJoin MapJoin(LineJoinStyle? join)
    {
        return join switch
        {
            LineJoinStyleBevel => SKStrokeJoin.Bevel,
            LineJoinStyleMiter => SKStrokeJoin.Miter,
            _ => SKStrokeJoin.Round
        };
    }

    private static SKPath BuildPath(IReadOnlyList<IEdge> edges, bool closeContours)
    {
        var skPath = new SKPath { FillType = SKPathFillType.EvenOdd };
        AppendEdges(skPath, edges, closeContours);
        return skPath;
    }

    private static void AppendEdges(SKPath skPath, IReadOnlyList<IEdge> edges, bool closeContours = false)
    {
        var startX = float.NaN;
        var startY = float.NaN;
        var lastX = float.NaN;
        var lastY = float.NaN;

        foreach (var edge in edges)
        {
            var fromX = edge.FromX / 20f;
            var fromY = edge.FromY / 20f;

            if (fromX != lastX || fromY != lastY)
            {
                if (closeContours && startX == lastX && startY == lastY)
                    skPath.Close();

                skPath.MoveTo(fromX, fromY);
                startX = fromX;
                startY = fromY;
            }

            if (edge is CurvedEdge curve)
                skPath.QuadTo(curve.ControlX / 20f, curve.ControlY / 20f, curve.ToX / 20f, curve.ToY / 20f);
            else
                skPath.LineTo(edge.ToX / 20f, edge.ToY / 20f);

            lastX = edge.ToX / 20f;
            lastY = edge.ToY / 20f;
        }

        if (closeContours && startX == lastX && startY == lastY)
            skPath.Close();
    }

    private static SKPath? BuildClipPath(IDrawable drawable)
    {
        if (drawable is not ShapeDefinition definition)
            return null;

        var skPath = new SKPath { FillType = SKPathFillType.EvenOdd };

        foreach (var path in definition.Shape.Paths)
            if (path.Style.Fill is not null)
                AppendEdges(skPath, path.Edges);

        return skPath;
    }

    private static SKShader LinearShader(LinearGradientFill fill)
    {
        var (colors, positions) = Stops(fill.Gradient);

        return SKShader.CreateLinearGradient(
            new SKPoint(-GradientSquare, 0),
            new SKPoint(GradientSquare, 0),
            colors,
            positions,
            SKShaderTileMode.Clamp,
            LocalMatrix(fill.Matrix));
    }

    private static SKShader RadialShader(RadialGradientFill fill)
    {
        var (colors, positions) = Stops(fill.Gradient);
        var matrix = LocalMatrix(fill.Matrix);

        if (fill.Gradient.FocalPoint is { } focal)
        {
            return SKShader.CreateTwoPointConicalGradient(
                new SKPoint(0, focal * GradientSquare),
                0,
                new SKPoint(0, 0),
                GradientSquare,
                colors,
                positions,
                SKShaderTileMode.Clamp,
                matrix);
        }

        return SKShader.CreateRadialGradient(
            new SKPoint(0, 0),
            GradientSquare,
            colors,
            positions,
            SKShaderTileMode.Clamp,
            matrix);
    }

    private static (SKColor[] Colors, float[] Positions) Stops(Gradient gradient)
    {
        var colors = new SKColor[gradient.Stops.Count];
        var positions = new float[gradient.Stops.Count];

        for (var i = 0; i < gradient.Stops.Count; i++)
        {
            colors[i] = ToSkColor(gradient.Stops[i].Color);
            positions[i] = gradient.Stops[i].Ratio / 255f;
        }

        return (colors, positions);
    }

    private static SKMatrix LocalMatrix(Matrix matrix)
    {
        return new SKMatrix(
            matrix.Scale.X.ToSingle(),
            matrix.Rotation.Y.ToSingle(),
            matrix.Translation.X / 20f,
            matrix.Rotation.X.ToSingle(),
            matrix.Scale.Y.ToSingle(),
            matrix.Translation.Y / 20f,
            0,
            0,
            1);
    }

    private static SKMatrix BitmapMatrix(Matrix matrix)
    {
        return new SKMatrix(
            matrix.Scale.X.ToSingle() / 20f,
            matrix.Rotation.Y.ToSingle() / 20f,
            matrix.Translation.X / 20f,
            matrix.Rotation.X.ToSingle() / 20f,
            matrix.Scale.Y.ToSingle() / 20f,
            matrix.Translation.Y / 20f,
            0,
            0,
            1);
    }

    private static SKColor ToSkColor(Color color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}
