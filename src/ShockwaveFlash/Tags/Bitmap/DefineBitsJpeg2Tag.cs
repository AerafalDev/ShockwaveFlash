// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags.Bitmap;

public sealed record DefineBitsJpeg2Tag(TagMetadata Metadata, ushort Id, SpanSlice Data) : Tag(Metadata)
{
    public static DefineBitsJpeg2Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new DefineBitsJpeg2Tag(metadata, reader.ReadUInt16(), reader.SliceToEnd());
    }
}
