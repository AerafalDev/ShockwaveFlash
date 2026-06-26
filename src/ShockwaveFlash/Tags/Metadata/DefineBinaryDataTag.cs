
namespace ShockwaveFlash.Tags.Metadata;

public sealed class DefineBinaryDataTag : Tag
{
    public ushort Id { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public DefineBinaryDataTag(TagMetadata metadata, ushort id, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Id = id;
        Data = data;
    }

    public static DefineBinaryDataTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        reader.Advance(sizeof(uint));
        var data = reader.ReadMemoryToEnd();

        return new DefineBinaryDataTag(metadata, id, data);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt32(0);
        writer.WriteMemory(Data);
    }
}
