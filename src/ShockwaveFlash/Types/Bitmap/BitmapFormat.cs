// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Bitmap;

public abstract record BitmapFormat
{
    public sealed record BitmapFormatColorMap8(byte NumColors) : BitmapFormat;

    public sealed record BitmapFormatRgb15 : BitmapFormat;

    public sealed record BitmapFormatRgb32 : BitmapFormat;

    public static BitmapFormat ColorMap8(byte numColors)
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
