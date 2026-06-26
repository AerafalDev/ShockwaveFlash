namespace ShockwaveFlash.Tags.Action;

public sealed class DoActionTag : Tag
{
    public ReadOnlyMemory<byte> Data { get; set; }

    public DoActionTag(TagMetadata metadata, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Data = data;
    }

    public static DoActionTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DoActionTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteMemory(Data);
    }
}
