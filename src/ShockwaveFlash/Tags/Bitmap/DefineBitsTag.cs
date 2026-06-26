
namespace ShockwaveFlash.Tags.Bitmap;

public sealed class DefineBitsTag : Tag
{
    public ushort Id { get; set; }

    public ReadOnlyMemory<byte> ImageData { get; set; }

    public DefineBitsTag(TagMetadata metadata, ushort id, ReadOnlyMemory<byte> imageData) : base(metadata)
    {
        Id = id;
        ImageData = imageData;
    }

    public static DefineBitsTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DefineBitsTag(metadata, reader.ReadUInt16(), reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteMemory(ImageData);
    }
}
