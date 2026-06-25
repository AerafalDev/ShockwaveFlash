using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags.Text;

public sealed record DefineTextTag(TagMetadata Metadata, ushort Id, Rectangle Bounds, Matrix Matrix, IReadOnlyList<TextRecord> Records) : Tag(Metadata)
{
    public static DefineTextTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var bounds = Rectangle.Decode(reader);
        var matrix = Matrix.Decode(reader);
        var numGlyphBits = reader.ReadUInt8();
        var numAdvanceBits = reader.ReadUInt8();

        var records = new List<TextRecord>();

        while (true)
        {
            var record = TextRecord.Decode(reader, 1, numGlyphBits, numAdvanceBits);

            if (record is null)
                break;

            records.Add(record);
        }

        return new DefineTextTag(metadata, id, bounds, matrix, records);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
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
            record.Encode(writer, 1, numGlyphBits, numAdvanceBits);

        writer.WriteUInt8(0);
    }
}
