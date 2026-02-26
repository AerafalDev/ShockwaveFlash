// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;
using ShockwaveFlash.Types.Bitmap;

namespace ShockwaveFlash.Tags;

public sealed record DefineBitsLossless2Tag(TagMetadata Metadata, ushort Id, ushort Width, ushort Height, BitmapFormat Format, SpanSlice ZLibBitmapData) : Tag(Metadata)
{
    public static DefineBitsLossless2Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var formatFlags = reader.ReadUInt8();
        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        var format = formatFlags switch
        {
            3 => BitmapFormat.ColorMap8(reader.ReadUInt8()),
            4 => throw new NotSupportedException("Invalid bitmap format."),
            5 => BitmapFormat.Rgb32(),
            _ => throw new NotSupportedException("Invalid bitmap format.")
        };
        var zlibBitmapData = reader.SliceToEnd();

        return new DefineBitsLossless2Tag(metadata, id, width, height, format, zlibBitmapData);
    }
}
