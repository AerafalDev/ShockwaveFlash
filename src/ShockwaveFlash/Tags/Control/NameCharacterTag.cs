// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.Control;

public sealed record NameCharacterTag(TagMetadata Metadata, ushort Id, string Name) : Tag(Metadata)
{
    public static NameCharacterTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var name = reader.ReadNullTerminatedString();

        return new NameCharacterTag(metadata, id, name);
    }
}
