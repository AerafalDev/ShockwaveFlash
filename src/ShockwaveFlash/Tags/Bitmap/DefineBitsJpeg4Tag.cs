// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags.Bitmap;

public sealed record DefineBitsJpeg4Tag(TagMetadata Metadata, ushort Id, float Deblocking, SpanSlice Data, SpanSlice AlphaData) : Tag(Metadata)
{
    public static DefineBitsJpeg4Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var dataLength = reader.ReadInt32();
        var deblocking = reader.ReadFixed8();
        var data = reader.Slice(dataLength);
        var alphaData = reader.SliceToEnd();

        return new DefineBitsJpeg4Tag(metadata, id, deblocking, data, alphaData);
    }
}
