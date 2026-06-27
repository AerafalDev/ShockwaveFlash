using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Types.Bitmap;

namespace ShockwaveFlash.Tags.Bitmap;

public sealed class DefineBitsLossless2Tag : Tag
{
    public ushort Id { get; set; }

    public ushort Width { get; set; }

    public ushort Height { get; set; }

    public BitmapFormat Format { get; set; }

    public ReadOnlyMemory<byte> ZLibBitmapData { get; set; }

    public DefineBitsLossless2Tag(TagMetadata metadata, ushort id, ushort width, ushort height, BitmapFormat format, ReadOnlyMemory<byte> zLibBitmapData) : base(metadata)
    {
        Id = id;
        Width = width;
        Height = height;
        Format = format;
        ZLibBitmapData = zLibBitmapData;
    }

    public static DefineBitsLossless2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var formatFlags = reader.ReadUInt8();
        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();
        var format = formatFlags switch
        {
            3 => BitmapFormat.ColorMap8(reader.ReadUInt8() + 1),
            5 => BitmapFormat.Rgb32(),
            _ => throw new SwfFormatException($"Invalid bitmap format {formatFlags}.")
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
            _ => throw new SwfUnsupportedException("Invalid bitmap format.")
        };

        writer.WriteUInt8(formatFlags);
        writer.WriteUInt16(Width);
        writer.WriteUInt16(Height);

        if (Format is BitmapFormat.BitmapFormatColorMap8 colorMap)
            writer.WriteUInt8((byte)(colorMap.NumColors - 1));

        writer.WriteMemory(ZLibBitmapData);
    }
}
