// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags.Font;

public sealed record DefineFontTag(TagMetadata Metadata, ushort Id, ushort[] OffsetTable, IReadOnlyList<IReadOnlyList<ShapeRecord>> Glyphs) : Tag(Metadata)
{
    public static DefineFontTag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion)
    {
        var id = reader.ReadUInt16();
        var firstOffset = reader.ReadUInt16();
        var numGlyphs = firstOffset >> 1;

        var offsetTable = new ushort[numGlyphs];

        offsetTable[0] = firstOffset;

        for (var i = 1; i < numGlyphs; i++)
            offsetTable[i] = reader.ReadUInt16();

        var glyphs = new List<IReadOnlyList<ShapeRecord>>(numGlyphs);

        for (var i = 0; i < numGlyphs; i++)
        {
            var glyph = new List<ShapeRecord>();
            var numBits = reader.ReadUInt8();
            var shapeContext = new ShapeContext(swfVersion, 1, numBits >> 4, numBits & 15);
            var bits = new BitReader();

            var record = ShapeRecord.Decode(reader, bits, shapeContext);
            glyph.Add(record);

            while (record is not EndShapeRecord)
            {
                record = ShapeRecord.Decode(reader, bits, shapeContext);
                glyph.Add(record);
            }

            glyphs.Add(glyph);
        }

        return new DefineFontTag(metadata, id, offsetTable, glyphs);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);

        foreach (var offset in OffsetTable)
            writer.WriteUInt16(offset);

        foreach (var glyph in Glyphs)
        {
            var numFillBits = 0;
            var numLineBits = 0;

            foreach (var record in glyph)
            {
                if (record is StyleChangeRecord styleChange)
                {
                    if (styleChange.FillStyle0 is { } fillStyle0)
                        numFillBits = Math.Max(numFillBits, BitWriter.UnsignedBitsNeeded(fillStyle0));

                    if (styleChange.FillStyle1 is { } fillStyle1)
                        numFillBits = Math.Max(numFillBits, BitWriter.UnsignedBitsNeeded(fillStyle1));

                    if (styleChange.LineStyle is { } lineStyle)
                        numLineBits = Math.Max(numLineBits, BitWriter.UnsignedBitsNeeded(lineStyle));
                }
            }

            writer.WriteUInt8((byte)((numFillBits << 4) | numLineBits));

            var context = new ShapeContext(swfVersion, 1, numFillBits, numLineBits);
            var bits = new BitWriter();

            foreach (var record in glyph)
                record.Encode(writer, bits, context);

            bits.Flush(writer);
        }
    }
}
