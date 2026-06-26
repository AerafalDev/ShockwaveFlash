
namespace ShockwaveFlash.Tags.Bitmap;

public sealed class JpegTablesTag : Tag
{
    public ReadOnlyMemory<byte> Data { get; set; }

    public JpegTablesTag(TagMetadata metadata, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Data = data;
    }

    public static JpegTablesTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new JpegTablesTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteMemory(Data);
    }
}
