using ShockwaveFlash.Rendering.Exceptions;
using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Model.Sprites;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Filter;
using ShockwaveFlash.Types.Shape;
using SkiaSharp;

namespace ShockwaveFlash.Rendering.Drawing.Skia;

public sealed class SkiaDrawer : IDrawer<SKImage>, IDisposable
{
    private const float GradientSquare = 819.2f;

    private const float SigmaFactor = 0.2886751f;

    private readonly SKBitmap? _bitmap;

    private readonly SKCanvas _canvas;

    private readonly bool _ownsCanvas;

    public SkiaDrawer(Rectangle bounds, float scale = 1f, SKColor? background = null)
    {
        var width = Math.Max(1, (int)Math.Ceiling(bounds.Width / 20.0 * scale));
        var height = Math.Max(1, (int)Math.Ceiling(bounds.Height / 20.0 * scale));

        _bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        _canvas = new SKCanvas(_bitmap);
        _ownsCanvas = true;
        Initialize(bounds, scale, background);
    }

    private SkiaDrawer(SKCanvas canvas, Rectangle bounds, float scale, SKColor? background)
    {
        _canvas = canvas;
        Initialize(bounds, scale, background);
    }

    private void Initialize(Rectangle bounds, float scale, SKColor? background)
    {
        _canvas.Clear(background ?? SKColors.Transparent);
        _canvas.Scale(scale);
        _canvas.Translate(-bounds.XMin / 20f, -bounds.YMin / 20f);
    }

    public static byte[] RenderToPng(IDrawable drawable, float scale = 1f, SKColor? background = null, int frame = 0)
    {
        return Encode(drawable, SKEncodedImageFormat.Png, 100, scale, background, frame);
    }

    public static byte[] RenderToJpeg(IDrawable drawable, int quality = 90, float scale = 1f, SKColor? background = null, int frame = 0)
    {
        return Encode(drawable, SKEncodedImageFormat.Jpeg, quality, scale, background, frame);
    }

    public static byte[] RenderToWebp(IDrawable drawable, int quality = 90, float scale = 1f, SKColor? background = null, int frame = 0)
    {
        return Encode(drawable, SKEncodedImageFormat.Webp, quality, scale, background, frame);
    }

    private static byte[] Encode(IDrawable drawable, SKEncodedImageFormat format, int quality, float scale, SKColor? background, int frame)
    {
        using var drawer = new SkiaDrawer(drawable.Bounds, scale, background);
        drawable.Draw(drawer, frame);

        using var image = drawer.Render();
        using var data = image.Encode(format, quality)
            ?? throw new RenderingException($"The native Skia build cannot encode {format} images.");

        return data.ToArray();
    }

    public static byte[] RenderToAnimatedGif(IDrawable drawable, int delayMilliseconds = 60, float scale = 1f, SKColor? background = null)
    {
        var bounds = drawable.Bounds;
        var width = Math.Max(1, (int)Math.Ceiling(bounds.Width / 20.0 * scale));
        var height = Math.Max(1, (int)Math.Ceiling(bounds.Height / 20.0 * scale));
        var frameCount = Math.Max(1, drawable.FrameCount(recursive: true));

        var frames = new List<SKColor[]>(frameCount);

        for (var frame = 0; frame < frameCount; frame++)
        {
            using var drawer = new SkiaDrawer(bounds, scale, background);
            drawable.Draw(drawer, frame);

            using var image = drawer.Render();
            using var bitmap = SKBitmap.FromImage(image);
            frames.Add(bitmap.Pixels);
        }

        return GifEncoder.Encode(frames, width, height, Math.Max(1, delayMilliseconds / 10));
    }

    public static byte[] RenderToPdf(IDrawable drawable, float scale = 1f, SKColor? background = null, int frame = 0)
    {
        var bounds = drawable.Bounds;
        var width = Math.Max(1f, (float)(bounds.Width / 20.0 * scale));
        var height = Math.Max(1f, (float)(bounds.Height / 20.0 * scale));

        using var stream = new MemoryStream();

        using (var document = SKDocument.CreatePdf(stream))
        {
            var canvas = document.BeginPage(width, height);

            using (var drawer = new SkiaDrawer(canvas, bounds, scale, background))
                drawable.Draw(drawer, frame);

            document.EndPage();
        }

        return stream.ToArray();
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
            using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            DrawWithFill(fillPath, fill, paint);
        }

        if (style.LineFill is { } lineFill)
        {
            using var strokePath = BuildPath(path.Edges, true);
            using var paint = StrokePaint(style);
            DrawWithFill(strokePath, lineFill, paint);
        }
        else if (style.LineColor is { } lineColor)
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

        var created = new List<IDisposable>();
        var imageFilter = BuildImageFilter(filters, created);
        var blend = MapBlend(blendMode);

        if (imageFilter is not null || blend != SKBlendMode.SrcOver || blendMode is BlendMode.Layer)
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
            BlendMode.Add or BlendMode.Screen => SKBlendMode.Screen,
            BlendMode.Lighten => SKBlendMode.Lighten,
            BlendMode.Darken or BlendMode.Subtract => SKBlendMode.Darken,
            BlendMode.Difference => SKBlendMode.Difference,
            BlendMode.Overlay => SKBlendMode.Overlay,
            BlendMode.HardLight => SKBlendMode.HardLight,
            BlendMode.Alpha => SKBlendMode.DstIn,
            BlendMode.Erase => SKBlendMode.DstOut,
            _ => SKBlendMode.SrcOver
        };
    }

    private static SKImageFilter? BuildImageFilter(IReadOnlyList<Filter> filters, List<IDisposable> created)
    {
        SKImageFilter? current = null;

        foreach (var filter in filters)
        {
            SKImageFilter? next = filter switch
            {
                BlurFilter blur => SKImageFilter.CreateBlur(Sigma(blur.Blur.X, blur.Passes), Sigma(blur.Blur.Y, blur.Passes), current),
                GlowFilter glow => glow.IsInner
                    ? InnerShadow(0, 0, Sigma(glow.Blur.X, glow.Passes), ToSkColor(glow.Color), glow.Strength.ToSingle(), current, created)
                    : SKImageFilter.CreateDropShadow(0, 0, Sigma(glow.Blur.X, glow.Passes), Sigma(glow.Blur.Y, glow.Passes), Strength(glow.Color, glow.Strength), current),
                DropShadowFilter shadow => shadow.IsInner
                    ? InnerShadow(Offset(shadow, true), Offset(shadow, false), Sigma(shadow.Blur.X, shadow.Passes), ToSkColor(shadow.Color), shadow.Strength.ToSingle(), current, created)
                    : SKImageFilter.CreateDropShadow(Offset(shadow, true), Offset(shadow, false), Sigma(shadow.Blur.X, shadow.Passes), Sigma(shadow.Blur.Y, shadow.Passes), Strength(shadow.Color, shadow.Strength), current),
                ColorMatrixFilter matrix when !matrix.Impotent => ColorMatrix(matrix.Matrix, current, created),
                _ => null
            };

            if (next is null)
                continue;

            created.Add(next);
            current = next;
        }

        return current;
    }

    private static SKImageFilter ColorMatrix(float[] matrix, SKImageFilter? input, List<IDisposable> created)
    {
        var values = (float[])matrix.Clone();
        values[4] /= 255f;
        values[9] /= 255f;
        values[14] /= 255f;
        values[19] /= 255f;

        var colorFilter = SKColorFilter.CreateColorMatrix(values);
        created.Add(colorFilter);

        return SKImageFilter.CreateColorFilter(colorFilter, input);
    }

    private static float Offset(DropShadowFilter shadow, bool horizontal)
    {
        var distance = shadow.Distance.ToSingle();
        var angle = shadow.Angle.ToSingle();
        return distance * (horizontal ? (float)Math.Cos(angle) : (float)Math.Sin(angle));
    }

    private static SKImageFilter InnerShadow(float dx, float dy, float sigma, SKColor color, float strength, SKImageFilter? source, List<IDisposable> created)
    {
        var moved = source;

        if (sigma > 0)
        {
            moved = SKImageFilter.CreateBlur(sigma, sigma, source);
            created.Add(moved);
        }

        if (dx != 0 || dy != 0)
        {
            moved = SKImageFilter.CreateOffset(dx, dy, moved);
            created.Add(moved);
        }

        var floodFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcOut);
        created.Add(floodFilter);

        var flood = SKImageFilter.CreateColorFilter(floodFilter, moved);
        created.Add(flood);

        if (strength is not 1f)
        {
            var gain = SKColorFilter.CreateColorMatrix(
            [
                1, 0, 0, 0, 0,
                0, 1, 0, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 0, strength, 0
            ]);
            created.Add(gain);

            flood = SKImageFilter.CreateColorFilter(gain, flood);
            created.Add(flood);
        }

        var rim = SKImageFilter.CreateBlendMode(SKBlendMode.SrcIn, source, flood);
        created.Add(rim);

        return SKImageFilter.CreateBlendMode(SKBlendMode.SrcOver, source, rim);
    }

    private static float Sigma(Fixed16 blur, int passes)
    {
        var width = blur.ToSingle();

        return width <= 1f || passes <= 0
            ? 0f
            : width * SigmaFactor * MathF.Sqrt(passes);
    }

    private static SKColor Strength(Color color, Fixed8 strength)
    {
        var alpha = (int)Math.Round(color.A * strength.ToSingle());
        return new SKColor(color.R, color.G, color.B, (byte)Math.Clamp(alpha, 0, 255));
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
        return SKImage.FromBitmap(_bitmap ?? throw new InvalidOperationException("This drawer renders to an external canvas and has no backing bitmap."));
    }

    public void Dispose()
    {
        if (_ownsCanvas)
            _canvas.Dispose();

        _bitmap?.Dispose();
    }

    private void DrawWithFill(SKPath skPath, IFillStyle fill, SKPaint paint)
    {
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
        using var paint = StrokePaint(style);
        paint.Color = color;
        _canvas.DrawPath(skPath, paint);
    }

    private static SKPaint StrokePaint(PathStyle style)
    {
        return new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Math.Max(style.LineWidth / 20f, 0.05f),
            StrokeCap = MapCap(style.LineCap),
            StrokeJoin = MapJoin(style.LineJoin),
            StrokeMiter = Math.Max(style.MiterLimit, 1f)
        };
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
        var skPath = new SKPath { FillType = SKPathFillType.Winding };
        AppendClip(skPath, drawable, SKMatrix.CreateIdentity(), 0);

        return skPath.IsEmpty ? null : skPath;
    }

    private static void AppendClip(SKPath skPath, IDrawable drawable, SKMatrix matrix, int depth)
    {
        if (depth > 8)
            return;

        switch (drawable)
        {
            case ShapeDefinition definition:
                using (var sub = new SKPath())
                {
                    foreach (var path in definition.Shape.Paths)
                        if (path.Style.Fill is not null)
                            AppendEdges(sub, path.Edges);

                    sub.Transform(matrix);
                    skPath.AddPath(sub);
                }

                break;

            case SpriteDefinition sprite:
                var frames = sprite.Timeline.Frames;

                if (frames.Count > 0)
                    foreach (var item in frames[0].Objects)
                        if (item.ClipDepth is null)
                            AppendClip(skPath, item.Drawable, SKMatrix.Concat(matrix, LocalMatrix(item.Matrix)), depth + 1);

                break;

            default:
                break;
        }
    }

    private static SKShader LinearShader(LinearGradientFill fill)
    {
        var (colors, positions) = Stops(fill.Gradient);

        return SKShader.CreateLinearGradient(
            new SKPoint(-GradientSquare, 0),
            new SKPoint(GradientSquare, 0),
            colors,
            positions,
            TileMode(fill.Gradient.Spread),
            LocalMatrix(fill.Matrix));
    }

    private static SKShader RadialShader(RadialGradientFill fill)
    {
        var (colors, positions) = Stops(fill.Gradient);
        var matrix = LocalMatrix(fill.Matrix);
        var tile = TileMode(fill.Gradient.Spread);

        if (fill.Gradient.FocalPoint is { } focal)
        {
            return SKShader.CreateTwoPointConicalGradient(
                new SKPoint(focal * GradientSquare, 0),
                0,
                new SKPoint(0, 0),
                GradientSquare,
                colors,
                positions,
                tile,
                matrix);
        }

        return SKShader.CreateRadialGradient(
            new SKPoint(0, 0),
            GradientSquare,
            colors,
            positions,
            tile,
            matrix);
    }

    private static SKShaderTileMode TileMode(GradientSpread spread)
    {
        return spread switch
        {
            GradientSpread.Reflect => SKShaderTileMode.Mirror,
            GradientSpread.Repeat => SKShaderTileMode.Repeat,
            _ => SKShaderTileMode.Clamp
        };
    }

    private static (SKColor[] Colors, float[] Positions) Stops(Gradient gradient)
    {
        if (gradient.Interpolation is GradientInterpolation.LinearRgb && gradient.Stops.Count > 1)
        {
            const int samples = 64;
            var rampColors = new SKColor[samples];
            var rampPositions = new float[samples];

            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)(samples - 1);
                rampPositions[i] = t;
                rampColors[i] = ToSkColor(GradientSampler.Sample(gradient.Stops, t));
            }

            return (rampColors, rampPositions);
        }

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
