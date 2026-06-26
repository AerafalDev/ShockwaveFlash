using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed class StartSoundTag : Tag
{
    public ushort Id { get; set; }

    public SoundInfo SoundInfo { get; set; }

    public StartSoundTag(TagMetadata metadata, ushort id, SoundInfo soundInfo) : base(metadata)
    {
        Id = id;
        SoundInfo = soundInfo;
    }

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
