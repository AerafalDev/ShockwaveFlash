
namespace ShockwaveFlash.Tags.Bitmap;

public sealed class DefineBitsJpeg3Tag : Tag
{
    public ushort Id { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public ReadOnlyMemory<byte> AlphaData { get; set; }

    public DefineBitsJpeg3Tag(TagMetadata metadata, ushort id, ReadOnlyMemory<byte> data, ReadOnlyMemory<byte> alphaData) : base(metadata)
    {
        Id = id;
        Data = data;
        AlphaData = alphaData;
    }

    public static DefineBitsJpeg3Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var dataLength = reader.ReadInt32();
        var data = reader.ReadMemory(dataLength);
        var alphaData = reader.ReadMemoryToEnd();

        return new DefineBitsJpeg3Tag(metadata, id, data, alphaData);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteInt32(Data.Length);
        writer.WriteMemory(Data);
        writer.WriteMemory(AlphaData);
    }
}
