// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags.Bitmap;

public sealed record DefineBitsTag(TagMetadata Metadata, ushort Id, SpanSlice ImageData) : Tag(Metadata)
{
    public static DefineBitsTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new DefineBitsTag(metadata, reader.ReadUInt16(), reader.SliceToEnd());
    }
}
