using ShockwaveFlash.Types;
using SkiaSharp;

namespace ShockwaveFlash.Rendering.Model.Images;

public sealed class RasterImage : IImage
{
    private readonly ReadOnlyMemory<byte> _png;

    private readonly int _width;

    private readonly int _height;

    public Rectangle Bounds => new(0, _width * 20, 0, _height * 20);

    public RasterImage(ReadOnlyMemory<byte> png, int width, int height)
    {
        _png = png;
        _width = width;
        _height = height;
    }

    public ReadOnlyMemory<byte> ToPng()
    {
        return _png;
    }

    public string ToBase64Data()
    {
        return "data:image/png;base64," + Convert.ToBase64String(_png.Span);
    }

    public IImage TransformColors(ColorTransform colorTransform)
    {
        using var source = SKBitmap.Decode(_png.ToArray());
        using var result = new SKBitmap(source.Width, source.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using var canvas = new SKCanvas(result);
        using var filter = ColorMatrix(colorTransform);
        using var paint = new SKPaint { ColorFilter = filter };

        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(source, 0, 0, paint);

        using var image = SKImage.FromBitmap(result);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(data.ToArray(), _width, _height);
    }

    private static SKColorFilter ColorMatrix(ColorTransform colorTransform)
    {
        return SKColorFilter.CreateColorMatrix(
        [
            colorTransform.RMult / 256f, 0, 0, 0, colorTransform.RAdd / 255f,
            0, colorTransform.GMult / 256f, 0, 0, colorTransform.GAdd / 255f,
            0, 0, colorTransform.BMult / 256f, 0, colorTransform.BAdd / 255f,
            0, 0, 0, colorTransform.AMult / 256f, colorTransform.AAdd / 255f
        ]);
    }
}
