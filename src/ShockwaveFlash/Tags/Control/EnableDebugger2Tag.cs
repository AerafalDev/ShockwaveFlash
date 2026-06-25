// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.Control;

public sealed record EnableDebugger2Tag(TagMetadata Metadata, string Password) : Tag(Metadata)
{
    public static EnableDebugger2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        reader.Advance(sizeof(ushort));

        return new EnableDebugger2Tag(metadata, reader.ReadNullTerminatedString());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(0);
        writer.WriteNullTerminatedString(Password);
    }
}
