namespace ShockwaveFlash.Tags.Action;

public sealed class DoInitActionTag : Tag
{
    public ushort Id { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public DoInitActionTag(TagMetadata metadata, ushort id, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Id = id;
        Data = data;
    }

    public static DoInitActionTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DoInitActionTag(metadata, reader.ReadUInt16(), reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteMemory(Data);
    }
}
