namespace ShockwaveFlash.Tags;

public sealed class UnknownTag : Tag
{
    public ReadOnlyMemory<byte> Data { get; set; }

    public UnknownTag(TagMetadata metadata, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Data = data;
    }

    public static UnknownTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new UnknownTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteMemory(Data);
    }
}
