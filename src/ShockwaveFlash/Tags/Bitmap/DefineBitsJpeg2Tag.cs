
namespace ShockwaveFlash.Tags.Bitmap;

public sealed class DefineBitsJpeg2Tag : Tag
{
    public ushort Id { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public DefineBitsJpeg2Tag(TagMetadata metadata, ushort id, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Id = id;
        Data = data;
    }

    public static DefineBitsJpeg2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DefineBitsJpeg2Tag(metadata, reader.ReadUInt16(), reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteMemory(Data);
    }
}
