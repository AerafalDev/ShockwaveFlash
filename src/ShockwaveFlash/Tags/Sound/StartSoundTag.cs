using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed record StartSoundTag(TagMetadata Metadata, ushort Id, SoundInfo SoundInfo) : Tag(Metadata)
{
    public static StartSoundTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var soundInfo = SoundInfo.Decode(reader);

        return new StartSoundTag(metadata, id, soundInfo);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        SoundInfo.Encode(writer);
    }
}
