
namespace ShockwaveFlash.Tags.Sound;

public sealed record SoundStreamBlockTag(TagMetadata Metadata, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static SoundStreamBlockTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new SoundStreamBlockTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteMemory(Data);
    }
}
