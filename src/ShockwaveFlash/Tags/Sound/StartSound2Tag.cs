using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed class StartSound2Tag : Tag
{
    public string ClassName { get; set; }

    public SoundInfo SoundInfo { get; set; }

    public StartSound2Tag(TagMetadata metadata, string className, SoundInfo soundInfo) : base(metadata)
    {
        ClassName = className;
        SoundInfo = soundInfo;
    }

    public static StartSound2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var className = reader.ReadNullTerminatedString();
        var soundInfo = SoundInfo.Decode(reader);

        return new StartSound2Tag(metadata, className, soundInfo);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteNullTerminatedString(ClassName);
        SoundInfo.Encode(writer);
    }
}
