// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags;

public sealed record DefineSpriteTag(TagMetadata Metadata, ushort Id, ushort NumFrames, IReadOnlyList<Tag> Tags) : Tag(Metadata)
{
    public static DefineSpriteTag Decode(ref SpanReader reader, TagMetadata metadata, byte swfVersion)
    {
        var id = reader.ReadUInt16();
        var numFrames = reader.ReadUInt16();
        var tags = DecodeCollection(ref reader, swfVersion);

        return new DefineSpriteTag(metadata, id, numFrames, tags);
    }
}
