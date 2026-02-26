// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags;

public sealed record DefineFontNameTag(TagMetadata Metadata, ushort Id, string FontName, string Copyright) : Tag(Metadata)
{
    public static DefineFontNameTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var name = reader.ReadNullTerminatedString();
        var copyright = reader.ReadNullTerminatedString();

        return new DefineFontNameTag(metadata, id, name, copyright);
    }
}
