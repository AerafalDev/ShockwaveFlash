namespace ShockwaveFlash.Types.Bitmap;

public abstract class BitmapFormat
{
    public sealed class BitmapFormatColorMap8 : BitmapFormat
    {
        public int NumColors { get; set; }

        public BitmapFormatColorMap8(int numColors)
        {
            NumColors = numColors;
        }
    }

    public sealed class BitmapFormatRgb15 : BitmapFormat
    {
    }

    public sealed class BitmapFormatRgb32 : BitmapFormat
    {
    }

    public static BitmapFormat ColorMap8(int numColors)
    {
        return new BitmapFormatColorMap8(numColors);
    }

    public static BitmapFormat Rgb15()
    {
        return new BitmapFormatRgb15();
    }

    public static BitmapFormat Rgb32()
    {
        return new BitmapFormatRgb32();
    }
}
