
namespace ShockwaveFlash.Tags.Sound;

public sealed class SoundStreamBlockTag : Tag
{
    public ReadOnlyMemory<byte> Data { get; set; }

    public SoundStreamBlockTag(TagMetadata metadata, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Data = data;
    }

    public static SoundStreamBlockTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new SoundStreamBlockTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteMemory(Data);
    }
}
