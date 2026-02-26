// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags;

public sealed record JpegTablesTag(TagMetadata Metadata, SpanSlice Data) : Tag(Metadata)
{
    public static JpegTablesTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new JpegTablesTag(metadata, reader.SliceToEnd());
    }
}
