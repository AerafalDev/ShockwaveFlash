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

        for (var y = 0; y < source.Height; y++)
        {
            for (var x = 0; x < source.Width; x++)
            {
                var pixel = source.GetPixel(x, y);

                if (pixel.Alpha == 0)
                {
                    result.SetPixel(x, y, pixel);
                    continue;
                }

                result.SetPixel(x, y, new SKColor(
                    Apply(pixel.Red, colorTransform.RMult, colorTransform.RAdd),
                    Apply(pixel.Green, colorTransform.GMult, colorTransform.GAdd),
                    Apply(pixel.Blue, colorTransform.BMult, colorTransform.BAdd),
                    Apply(pixel.Alpha, colorTransform.AMult, colorTransform.AAdd)));
            }
        }

        using var image = SKImage.FromBitmap(result);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(data.ToArray(), _width, _height);
    }

    private static byte Apply(byte channel, int multiplier, int addend)
    {
        var value = (channel * multiplier / 256) + addend;
        return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
    }
}
