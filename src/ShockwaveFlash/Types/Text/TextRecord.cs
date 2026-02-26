// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Text;

public sealed record TextRecord(ushort? Id, Color? Color, Point? Offset, ushort? Height, Glyph[] Glyphs)
{
    public static TextRecord? Decode(ref SpanReader reader, byte tagVersion, byte numGlyphBits, byte numAdvanceBits)
    {
        var flags = reader.ReadUInt8();

        if (flags is 0)
            return null;

        ushort? characterId = (flags & 8) is not 0
            ? reader.ReadUInt16()
            : null;

        Color? color = (flags & 4) is not 0
            ? tagVersion is 1
                ? Types.Color.DecodeRgb(ref reader)
                : Types.Color.DecodeRgba(ref reader)
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
            glyphs[i] = new Glyph(bits.ReadUBits(ref reader, numGlyphBits), bits.ReadSBits(ref reader, numAdvanceBits));

        return new TextRecord(characterId, color, offset, height, glyphs);
    }
}
