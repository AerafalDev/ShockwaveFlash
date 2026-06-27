using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags.Text;

public class DefineTextTag : Tag
{
    public ushort Id { get; set; }

    public Rectangle Bounds { get; set; }

    public Matrix Matrix { get; set; }

    public IReadOnlyList<TextRecord> Records { get; set; }

    public DefineTextTag(TagMetadata metadata, ushort id, Rectangle bounds, Matrix matrix, IReadOnlyList<TextRecord> records) : base(metadata)
    {
        Id = id;
        Bounds = bounds;
        Matrix = matrix;
        Records = records;
    }

    public static DefineTextTag Decode(MemoryReader reader, TagMetadata metadata, byte textVersion)
    {
        var id = reader.ReadUInt16();
        var bounds = Rectangle.Decode(reader);
        var matrix = Matrix.Decode(reader);
        var numGlyphBits = reader.ReadUInt8();
        var numAdvanceBits = reader.ReadUInt8();

        var records = new List<TextRecord>();

        while (true)
        {
            var record = TextRecord.Decode(reader, textVersion, numGlyphBits, numAdvanceBits);

            if (record is null)
                break;

            records.Add(record);
        }

        return textVersion switch
        {
            1 => new DefineTextTag(metadata, id, bounds, matrix, records),
            2 => new DefineText2Tag(metadata, id, bounds, matrix, records),
            _ => throw new SwfFormatException($"Text version {textVersion} is not supported.")
        };
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        EncodeBody(writer, 1);
    }

    protected void EncodeBody(MemoryWriter writer, byte textVersion)
    {
        writer.WriteUInt16(Id);
        Bounds.Encode(writer);
        Matrix.Encode(writer);

        byte numGlyphBits = 0;
        byte numAdvanceBits = 0;

        foreach (var record in Records)
        {
            foreach (var glyph in record.Glyphs)
            {
                numGlyphBits = (byte)Math.Max(numGlyphBits, BitWriter.UnsignedBitsNeeded(glyph.Index));
                numAdvanceBits = (byte)Math.Max(numAdvanceBits, BitWriter.SignedBitsNeeded(glyph.Advance));
            }
        }

        writer.WriteUInt8(numGlyphBits);
        writer.WriteUInt8(numAdvanceBits);

        foreach (var record in Records)
            record.Encode(writer, textVersion, numGlyphBits, numAdvanceBits);

        writer.WriteUInt8(0);
    }
}
