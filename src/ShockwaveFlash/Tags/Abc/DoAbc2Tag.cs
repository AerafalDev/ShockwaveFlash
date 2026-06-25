// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Abc;

namespace ShockwaveFlash.Tags.Abc;

public sealed record DoAbc2Tag(TagMetadata Metadata, DoAbc2Flags Flags, string Name, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static DoAbc2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var flags = (DoAbc2Flags)reader.ReadUInt32();
        var name = reader.ReadNullTerminatedString();
        var data = reader.ReadMemoryToEnd();

        return new DoAbc2Tag(metadata, flags, name, data);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt32((uint)Flags);
        writer.WriteNullTerminatedString(Name);
        writer.WriteMemory(Data);
    }
}
