using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed record StartSound2Tag(TagMetadata Metadata, string ClassName, SoundInfo SoundInfo) : Tag(Metadata)
{
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
