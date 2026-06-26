namespace ShockwaveFlash.Types.Text;

public sealed class TextRecord
{
    public ushort? Id { get; set; }

    public Color? Color { get; set; }

    public short? OffsetX { get; set; }

    public short? OffsetY { get; set; }

    public ushort? Height { get; set; }

    public Glyph[] Glyphs { get; set; }

    public TextRecord(ushort? id, Color? color, short? offsetX, short? offsetY, ushort? height, Glyph[] glyphs)
    {
        Id = id;
        Color = color;
        OffsetX = offsetX;
        OffsetY = offsetY;
        Height = height;
        Glyphs = glyphs;
    }

    public static TextRecord? Decode(MemoryReader reader, byte tagVersion, byte numGlyphBits, byte numAdvanceBits)
    {
        var flags = reader.ReadUInt8();

        if (flags is 0)
            return null;

        ushort? id = (flags & 8) is not 0
            ? reader.ReadUInt16()
            : null;

        Color? color = (flags & 4) is not 0
            ? tagVersion is 1
                ? Types.Color.DecodeRgb(reader)
                : Types.Color.DecodeRgba(reader)
            : null;

        short? offsetX = (flags & 1) is not 0
            ? reader.ReadInt16()
            : null;

        short? offsetY = (flags & 2) is not 0
            ? reader.ReadInt16()
            : null;

        ushort? height = (flags & 8) is not 0
            ? reader.ReadUInt16()
            : null;

        var numGlyphs = reader.ReadUInt8();
        var glyphs = new Glyph[numGlyphs];
        var bits = new BitReader();

        for (var i = 0; i < numGlyphs; i++)
            glyphs[i] = new Glyph(bits.ReadUBits(reader, numGlyphBits), bits.ReadSBits(reader, numAdvanceBits));

        return new TextRecord(id, color, offsetX, offsetY, height, glyphs);
    }

    public void Encode(MemoryWriter writer, byte tagVersion, byte numGlyphBits, byte numAdvanceBits)
    {
        byte flags = 0x80;

        if (Id is not null && Height is not null)
            flags |= 8;

        if (Color is not null)
            flags |= 4;

        if (OffsetX is not null)
            flags |= 1;

        if (OffsetY is not null)
            flags |= 2;

        writer.WriteUInt8(flags);

        if ((flags & 8) is not 0 && Id is { } id)
            writer.WriteUInt16(id);

        if ((flags & 4) is not 0 && Color is { } color)
        {
            if (tagVersion is 1)
                color.EncodeRgb(writer);
            else
                color.EncodeRgba(writer);
        }

        if (OffsetX is { } offsetX)
            writer.WriteInt16(offsetX);

        if (OffsetY is { } offsetY)
            writer.WriteInt16(offsetY);

        if ((flags & 8) is not 0 && Height is { } height)
            writer.WriteUInt16(height);

        writer.WriteUInt8((byte)Glyphs.Length);

        var bits = new BitWriter();

        foreach (var glyph in Glyphs)
        {
            bits.WriteUBits(writer, glyph.Index, numGlyphBits);
            bits.WriteSBits(writer, glyph.Advance, numAdvanceBits);
        }

        bits.Flush(writer);
    }
}
