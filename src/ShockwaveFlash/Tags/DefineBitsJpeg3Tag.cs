// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags;

public sealed record DefineBitsJpeg3Tag(TagMetadata Metadata, ushort Id, SpanSlice Data, SpanSlice AlphaData) : Tag(Metadata)
{
    public static DefineBitsJpeg3Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var dataLength = reader.ReadInt32();
        var data = reader.Slice(dataLength);
        var alphaData = reader.SliceToEnd();

        return new DefineBitsJpeg3Tag(metadata, id, data, alphaData);
    }
}
