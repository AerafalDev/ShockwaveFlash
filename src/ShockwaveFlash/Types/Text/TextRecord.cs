// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Text;

public sealed record TextRecord(ushort? Id, Color? Color, Point? Offset, ushort? Height, Glyph[] Glyphs)
{
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

        Point? offset = offsetX is null && offsetY is null
            ? null
            : new Point(offsetX ?? 0, offsetY ?? 0);

        ushort? height = (flags & 8) is not 0
            ? reader.ReadUInt16()
            : null;

        var numGlyphs = reader.ReadUInt8();
        var glyphs = new Glyph[numGlyphs];
        var bits = new BitReader();

        for (var i = 0; i < numGlyphs; i++)
            glyphs[i] = new Glyph(bits.ReadUBits(reader, numGlyphBits), bits.ReadSBits(reader, numAdvanceBits));

        return new TextRecord(id, color, offset, height, glyphs);
    }

    public void Encode(MemoryWriter writer, byte tagVersion, byte numGlyphBits, byte numAdvanceBits)
    {
        byte flags = 0x80;

        if (Id is not null && Height is not null)
            flags |= 8;

        if (Color is not null)
            flags |= 4;

        if (Offset is not null)
            flags |= 1 | 2;

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

        if ((flags & 1) is not 0 && Offset is { } offsetX)
            writer.WriteInt16((short)offsetX.X);

        if ((flags & 2) is not 0 && Offset is { } offsetY)
            writer.WriteInt16((short)offsetY.Y);

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
