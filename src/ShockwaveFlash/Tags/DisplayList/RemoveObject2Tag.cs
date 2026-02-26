// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.DisplayList;

public sealed record RemoveObject2Tag(TagMetadata Metadata, ushort Depth) : Tag(Metadata)
{
    public static RemoveObject2Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new RemoveObject2Tag(metadata, reader.ReadUInt16());
    }
}
