
using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.Bitmap;

public sealed class DefineBitsJpeg4Tag : Tag
{
    public ushort Id { get; set; }

    public Fixed8 Deblocking { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public ReadOnlyMemory<byte> AlphaData { get; set; }

    public DefineBitsJpeg4Tag(TagMetadata metadata, ushort id, Fixed8 deblocking, ReadOnlyMemory<byte> data, ReadOnlyMemory<byte> alphaData) : base(metadata)
    {
        Id = id;
        Deblocking = deblocking;
        Data = data;
        AlphaData = alphaData;
    }

    public static DefineBitsJpeg4Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var dataLength = reader.ReadInt32();
        var deblocking = reader.ReadFixed8();
        var data = reader.ReadMemory(dataLength);
        var alphaData = reader.ReadMemoryToEnd();

        return new DefineBitsJpeg4Tag(metadata, id, deblocking, data, alphaData);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteInt32(Data.Length);
        writer.WriteFixed8(Deblocking);
        writer.WriteMemory(Data);
        writer.WriteMemory(AlphaData);
    }
}
