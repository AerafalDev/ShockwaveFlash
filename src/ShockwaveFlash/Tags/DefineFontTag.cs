// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags;

public sealed record DefineFontTag(TagMetadata Metadata, ushort Id, ushort[] OffsetTable, IReadOnlyList<IReadOnlyList<ShapeRecord>> Glyphs) : Tag(Metadata)
{
    public static DefineFontTag Decode(ref SpanReader reader, TagMetadata metadata, byte swfVersion)
    {
        var characterId = reader.ReadUInt16();
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

            var record = ShapeRecord.Decode(ref reader, ref bits, ref shapeContext);
            glyph.Add(record);

            while (record is not EndShapeRecord)
            {
                record = ShapeRecord.Decode(ref reader, ref bits, ref shapeContext);
                glyph.Add(record);
            }

            glyphs.Add(glyph);
        }

        return new DefineFontTag(metadata, characterId, offsetTable, glyphs);
    }
}
