// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed record DefineSoundTag(TagMetadata Metadata, ushort Id, SoundFormat Format, uint NumSamples, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static DefineSoundTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var format = SoundFormat.Decode(reader);
        var numSamples = reader.ReadUInt32();
        var data = reader.ReadMemoryToEnd();

        return new DefineSoundTag(metadata, id, format, numSamples, data);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        Format.Encode(writer);
        writer.WriteUInt32(NumSamples);
        writer.WriteMemory(Data);
    }
}
