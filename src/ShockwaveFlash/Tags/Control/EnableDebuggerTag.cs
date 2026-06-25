// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.Control;

public sealed record EnableDebuggerTag(TagMetadata Metadata, string Password) : Tag(Metadata)
{
    public static EnableDebuggerTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new EnableDebuggerTag(metadata, reader.ReadNullTerminatedString());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteNullTerminatedString(Password);
    }
}
