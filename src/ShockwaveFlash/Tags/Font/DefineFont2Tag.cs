using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Font;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags.Font;

public class DefineFont2Tag : Tag
{
    public ushort Id { get; set; }

    public string Name { get; set; }

    public Language Language { get; set; }

    public FontLayout? Layout { get; set; }

    public FontGlyph[] Glyphs { get; set; }

    public FontFlags Flags { get; set; }

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

    public DefineFont2Tag(TagMetadata metadata, ushort id, string name, Language language, FontLayout? layout, FontGlyph[] glyphs, FontFlags flags) : base(metadata)
    {
        Id = id;
        Name = name;
        Language = language;
        Layout = layout;
        Glyphs = glyphs;
        Flags = flags;
    }

    public static DefineFont2Tag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion, byte fontVersion)
    {
        var id = reader.ReadUInt16();
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
                var shape = ShapeRecord.Decode(reader, bits, context);

                while (shape is not EndShapeRecord)
                {
                    shapes.Add(shape);
                    shape = ShapeRecord.Decode(reader, bits, context);
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
                    glyphs[i].Bounds = Rectangle.Decode(reader);
            }

            FontKerning[] kerning = [];

            if (reader.Remaining > 0)
            {
                var numKerning = reader.ReadUInt16();
                kerning = new FontKerning[numKerning];

                for (var i = 0; i < numKerning; i++)
                    kerning[i] = FontKerning.Decode(reader, flags.HasFlag(FontFlags.HasWideCodes));
            }

            layout = new FontLayout(ascent, descent, leading, kerning);
        }

        return fontVersion switch
        {
            2 => new DefineFont2Tag(metadata, id, name, language, layout, glyphs, flags),
            3 => new DefineFont3Tag(metadata, id, name, language, layout, glyphs, flags),
            _ => throw new SwfFormatException($"Font version {fontVersion} is not supported.")
        };
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt8((byte)Flags);
        writer.WriteUInt8((byte)Language);
        writer.WriteLengthPrefixedString(Name);

        var numGlyphs = (ushort)Glyphs.Length;

        writer.WriteUInt16(numGlyphs);

        if (numGlyphs is 0)
        {
            if (HasWideOffsets)
                writer.WriteUInt32(0);
            else
                writer.WriteUInt16(0);

            return;
        }

        var offsetSize = HasWideOffsets ? sizeof(uint) : sizeof(ushort);
        var glyphData = new ReadOnlyMemory<byte>[numGlyphs];

        for (var i = 0; i < numGlyphs; i++)
            glyphData[i] = EncodeGlyphData(Glyphs[i], swfVersion);

        var offsets = new uint[numGlyphs];
        var current = (uint)(offsetSize * (numGlyphs + 1));

        for (var i = 0; i < numGlyphs; i++)
        {
            offsets[i] = current;
            current += (uint)glyphData[i].Length;
        }

        var codeTableOffset = current;

        for (var i = 0; i < numGlyphs; i++)
        {
            if (HasWideOffsets)
                writer.WriteUInt32(offsets[i]);
            else
                writer.WriteUInt16((ushort)offsets[i]);
        }

        if (HasWideOffsets)
            writer.WriteUInt32(codeTableOffset);
        else
            writer.WriteUInt16((ushort)codeTableOffset);

        for (var i = 0; i < numGlyphs; i++)
            writer.WriteMemory(glyphData[i]);

        for (var i = 0; i < numGlyphs; i++)
        {
            if (HasWideCodes)
                writer.WriteUInt16(Glyphs[i].Code);
            else
                writer.WriteUInt8((byte)Glyphs[i].Code);
        }

        if (HasLayout && Layout is { } layout)
        {
            writer.WriteUInt16(layout.Ascent);
            writer.WriteUInt16(layout.Descent);
            writer.WriteInt16(layout.Leading);

            for (var i = 0; i < numGlyphs; i++)
                writer.WriteUInt16(Glyphs[i].Advance);

            for (var i = 0; i < numGlyphs; i++)
                (Glyphs[i].Bounds ?? new Rectangle()).Encode(writer);

            writer.WriteUInt16((ushort)layout.Kerning.Length);

            foreach (var kerning in layout.Kerning)
                kerning.Encode(writer, HasWideCodes);
        }
    }

    private static ReadOnlyMemory<byte> EncodeGlyphData(FontGlyph glyph, byte swfVersion)
    {
        if (glyph.Shapes.Count is 0)
            return ReadOnlyMemory<byte>.Empty;

        var numFillBits = 0;
        var numLineBits = 0;

        foreach (var record in glyph.Shapes)
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

        var glyphWriter = new MemoryWriter();

        glyphWriter.WriteUInt8((byte)((numFillBits << 4) | numLineBits));

        var context = new ShapeContext(swfVersion, 1, numFillBits, numLineBits);
        var bits = new BitWriter();

        foreach (var record in glyph.Shapes)
            record.Encode(glyphWriter, bits, context);

        bits.Flush(glyphWriter);

        return glyphWriter.WrittenMemory;
    }
}
