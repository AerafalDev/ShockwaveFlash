
namespace ShockwaveFlash.Tags.Abc;

public sealed class DoAbcTag : Tag
{
    public ReadOnlyMemory<byte> Data { get; set; }

    public DoAbcTag(TagMetadata metadata, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Data = data;
    }

    public static DoAbcTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DoAbcTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteMemory(Data);
    }
}
