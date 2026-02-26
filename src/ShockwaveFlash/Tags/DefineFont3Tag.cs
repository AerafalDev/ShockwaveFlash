// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Font;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags;

public sealed record DefineFont3Tag(TagMetadata Metadata, ushort Id, string Name, Language Language, FontLayout? Layout, FontGlyph[] Glyphs, FontFlags Flags) : Tag(Metadata)
{
    public bool IsBold =>
        Flags.HasFlag(FontFlags.IsBold);

    public bool IsItalic =>
        Flags.HasFlag(FontFlags.IsItalic);

    public bool HasWideCodes =>
        Flags.HasFlag(FontFlags.HasWideCodes);

    public bool HasWideOffsets =>
        Flags.HasFlag(FontFlags.HasWideOffsets);

    public bool IsAnsi =>
        Flags.HasFlag(FontFlags.IsAnsi);

    public bool IsSmallText =>
        Flags.HasFlag(FontFlags.IsSmallText);

    public bool IsShiftJis =>
        Flags.HasFlag(FontFlags.IsShiftJis);

    public bool HasLayout =>
        Flags.HasFlag(FontFlags.HasLayout);

    public static DefineFont3Tag Decode(ref SpanReader reader, TagMetadata metadata, byte swfVersion)
    {
        var characterId = reader.ReadUInt16();
        var flags = (FontFlags)reader.ReadUInt8();
        var language = (Language)reader.ReadUInt8();
        var name = reader.ReadLengthPrefixedString();
        var numGlyphs = reader.ReadUInt16();
        var glyphs = new FontGlyph[numGlyphs];

        for (var i = 0; i < numGlyphs; i++)
            glyphs[i] = new FontGlyph([], 0, 0, null);

        if (numGlyphs is 0 && reader.Remaining > 0)
            reader.Advance(flags.HasFlag(FontFlags.HasWideOffsets) ? sizeof(uint) : sizeof(ushort));
        else if (numGlyphs > 0 && reader.Remaining > 0)
        {
            var offsets = new uint[numGlyphs];

            for (var i = 0; i < numGlyphs; i++)
                offsets[i] = flags.HasFlag(FontFlags.HasWideOffsets) ? reader.ReadUInt32() : reader.ReadUInt16();

            var codeTableOffset = flags.HasFlag(FontFlags.HasWideOffsets) ? reader.ReadUInt32() : reader.ReadUInt16();

            for (var i = 0; i < numGlyphs; i++)
            {
                var remaining = i < numGlyphs - 1 ?
                    offsets[i + 1] - offsets[i] :
                    codeTableOffset - offsets[i];

                if (remaining is 0)
                    continue;

                var numBits = reader.ReadUInt8();

                if (remaining is 1)
                    continue;

                var bits = new BitReader();
                var context = new ShapeContext(swfVersion, 1, numBits >> 4, numBits & 15);

                var shapes = new List<ShapeRecord>();
                var shape = ShapeRecord.Decode(ref reader, ref bits, ref context);

                while (shape is not EndShapeRecord)
                {
                    shapes.Add(shape);
                    shape = ShapeRecord.Decode(ref reader, ref bits, ref context);
                }
                shapes.Add(shape);

                glyphs[i].Shapes = shapes;
            }

            for (var i = 0; i < numGlyphs; i++)
                glyphs[i].Code = flags.HasFlag(FontFlags.HasWideCodes) ? reader.ReadUInt16() : reader.ReadUInt8();
        }

        FontLayout? layout = null;

        if (reader.Remaining > 0 && flags.HasFlag(FontFlags.HasLayout))
        {
            var ascent = reader.ReadUInt16();
            var descent = reader.ReadUInt16();
            var leading = reader.ReadInt16();

            for (var i = 0; i < numGlyphs; i++)
                glyphs[i].Advance = reader.ReadUInt16();

            if (reader.Remaining > 0)
            {
                for (var i = 0; i < numGlyphs; i++)
                    glyphs[i].Bounds = Rectangle.Decode(ref reader);
            }

            FontKerning[] kerning = [];

            if (reader.Remaining > 0)
            {
                var numKerning = reader.ReadUInt16();
                kerning = new FontKerning[numKerning];

                for (var i = 0; i < numKerning; i++)
                    kerning[i] = FontKerning.Decode(ref reader, flags.HasFlag(FontFlags.HasWideCodes));
            }

            layout = new FontLayout(ascent, descent, leading, kerning);
        }

        return new DefineFont3Tag(metadata, characterId, name, language, layout, glyphs, flags);
    }
}
