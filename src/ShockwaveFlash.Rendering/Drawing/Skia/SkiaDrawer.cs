using ShockwaveFlash.Rendering.Model.Images;
using ShockwaveFlash.Rendering.Model.Shapes;
using ShockwaveFlash.Rendering.Scene;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Filter;
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

        using var skPath = BuildPath(path.Edges);

        if (style.Fill is { } fill)
            FillPath(skPath, fill);

        if (style.LineColor is { } lineColor)
            StrokePath(skPath, ToSkColor(lineColor), style.LineWidth);
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
        drawable.Draw(this, frame);
        _canvas.Restore();
    }

    public string StartClip(IDrawable drawable, Matrix matrix, int frame)
    {
        throw new NotSupportedException("Clipping is not implemented yet.");
    }

    public void EndClip(string clipId)
    {
        throw new NotSupportedException("Clipping is not implemented yet.");
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

    private void StrokePath(SKPath skPath, SKColor color, int lineWidth)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = color,
            StrokeWidth = Math.Max(lineWidth / 20f, 0.05f),
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        _canvas.DrawPath(skPath, paint);
    }

    private static SKPath BuildPath(IReadOnlyList<IEdge> edges)
    {
        var skPath = new SKPath { FillType = SKPathFillType.EvenOdd };
        var lastX = float.NaN;
        var lastY = float.NaN;

        foreach (var edge in edges)
        {
            var fromX = edge.FromX / 20f;
            var fromY = edge.FromY / 20f;

            if (fromX != lastX || fromY != lastY)
                skPath.MoveTo(fromX, fromY);

            if (edge is CurvedEdge curve)
                skPath.QuadTo(curve.ControlX / 20f, curve.ControlY / 20f, curve.ToX / 20f, curve.ToY / 20f);
            else
                skPath.LineTo(edge.ToX / 20f, edge.ToY / 20f);

            lastX = edge.ToX / 20f;
            lastY = edge.ToY / 20f;
        }

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
