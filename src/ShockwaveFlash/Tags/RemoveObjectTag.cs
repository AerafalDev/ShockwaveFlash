// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags;

public sealed record RemoveObjectTag(TagMetadata Metadata, ushort Id, ushort Depth) : Tag(Metadata)
{
    public static RemoveObjectTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new RemoveObjectTag(metadata, reader.ReadUInt16(), reader.ReadUInt16());
    }
}
