// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.DisplayList;

public sealed record RemoveObject2Tag(TagMetadata Metadata, ushort Depth) : Tag(Metadata)
{
    public static RemoveObject2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new RemoveObject2Tag(metadata, reader.ReadUInt16());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Depth);
    }
}
