using ShockwaveFlash.Types.Bitmap;

namespace ShockwaveFlash.Tags.Bitmap;

public sealed record DefineBitsLossless2Tag(TagMetadata Metadata, ushort Id, ushort Width, ushort Height, BitmapFormat Format, ReadOnlyMemory<byte> ZLibBitmapData) : Tag(Metadata)
{
    public static DefineBitsLossless2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var formatFlags = reader.ReadUInt8();
        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        var format = formatFlags switch
        {
            3 => BitmapFormat.ColorMap8(reader.ReadUInt8()),
            4 => throw new NotSupportedException("Invalid bitmap format."),
            5 => BitmapFormat.Rgb32(),
            _ => throw new NotSupportedException("Invalid bitmap format.")
        };
        var zlibBitmapData = reader.ReadMemoryToEnd();

        return new DefineBitsLossless2Tag(metadata, id, width, height, format, zlibBitmapData);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);

        var formatFlags = Format switch
        {
            BitmapFormat.BitmapFormatColorMap8 => (byte)3,
            BitmapFormat.BitmapFormatRgb15 => (byte)4,
            BitmapFormat.BitmapFormatRgb32 => (byte)5,
            _ => throw new NotSupportedException("Invalid bitmap format.")
        };

        writer.WriteUInt8(formatFlags);
        writer.WriteUInt16(Width);
        writer.WriteUInt16(Height);

        if (Format is BitmapFormat.BitmapFormatColorMap8 colorMap)
            writer.WriteUInt8(colorMap.NumColors);

        writer.WriteMemory(ZLibBitmapData);
    }
}
