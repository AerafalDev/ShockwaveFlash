// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags;

public sealed record DefineBinaryDataTag(TagMetadata Metadata, ushort Id, SpanSlice Data) : Tag(Metadata)
{
    public static DefineBinaryDataTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        reader.Advance(sizeof(uint));
        var data = reader.SliceToEnd();

        return new DefineBinaryDataTag(metadata, id, data);
    }
}
