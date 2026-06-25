using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Morph;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags.Morph;

public sealed record DefineMorphShape2Tag(TagMetadata Metadata, ushort Id, DefineMorphShapeFlags Flags, MorphShape Start, MorphShape End) : Tag(Metadata)
{
    public bool HasScalingStrokes =>
        Flags.HasFlag(DefineMorphShapeFlags.HasScalingStrokes);

    public bool HasNonScalingStrokes =>
        Flags.HasFlag(DefineMorphShapeFlags.HasNonScalingStrokes);

    public static DefineMorphShape2Tag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion)
    {
        var id = reader.ReadUInt16();
        var startShapeBounds = Rectangle.Decode(reader);
        var endShapeBounds = Rectangle.Decode(reader);

        var startEdgeBounds = Rectangle.Decode(reader);
        var endEdgeBounds = Rectangle.Decode(reader);
        var flags = (DefineMorphShapeFlags)reader.ReadUInt8();

        reader.Advance(sizeof(int));

        var numFillStyles = reader.ReadUInt8() is var tempNumFillStyles && tempNumFillStyles is 255 ? reader.ReadUInt16() : tempNumFillStyles;

        var startFillStyles = new FillStyle[numFillStyles];
        var endFillStyles = new FillStyle[numFillStyles];

        for (var i = 0; i < numFillStyles; i++)
        {
            var (startFillStyle, endFillStyle) = FillStyle.DecodeMorph(reader);
            startFillStyles[i] = startFillStyle;
            endFillStyles[i] = endFillStyle;
        }

        var numLineStyles = reader.ReadUInt8() is var tempNumLineStyles && tempNumLineStyles is 255 ? reader.ReadUInt16() : tempNumLineStyles;

        var startLineStyles = new LineStyle[numLineStyles];
        var endLineStyles = new LineStyle[numLineStyles];

        for (var i = 0; i < numLineStyles; i++)
        {
            var (startLineStyle, endLineStyle) = LineStyle.DecodeMorph(reader, 2);
            startLineStyles[i] = startLineStyle;
            endLineStyles[i] = endLineStyle;
        }

        var bits = new BitReader();
        var context = new ShapeContext(swfVersion, 2, (byte)bits.ReadUBits(reader, 4), (byte)bits.ReadUBits(reader, 4));

        var startShapes = new List<ShapeRecord>();
        var shape = ShapeRecord.Decode(reader, bits, context);

        while (shape is not EndShapeRecord)
        {
            startShapes.Add(shape);
            shape = ShapeRecord.Decode(reader, bits, context);
        }

        startShapes.Add(shape);

        reader.Advance(sizeof(byte));

        bits = new BitReader();
        context = new ShapeContext(swfVersion, 2, 0, 0);

        var endShapes = new List<ShapeRecord>();
        shape = ShapeRecord.Decode(reader, bits, context);

        while (shape is not EndShapeRecord)
        {
            endShapes.Add(shape);
            shape = ShapeRecord.Decode(reader, bits, context);
        }

        endShapes.Add(shape);

        var startShape = new MorphShape(startShapeBounds, startEdgeBounds, startFillStyles, startLineStyles, startShapes);
        var endShape = new MorphShape(endShapeBounds, endEdgeBounds, endFillStyles, endLineStyles, endShapes);

        return new DefineMorphShape2Tag(metadata, id, flags, startShape, endShape);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        Start.ShapeBounds.Encode(writer);
        End.ShapeBounds.Encode(writer);
        Start.EdgeBounds.Encode(writer);
        End.EdgeBounds.Encode(writer);
        writer.WriteUInt8((byte)Flags);

        var body = new MemoryWriter();

        var numFillStyles = Start.FillStyles.Length;

        if (numFillStyles >= 255)
        {
            body.WriteUInt8(255);
            body.WriteUInt16((ushort)numFillStyles);
        }
        else
        {
            body.WriteUInt8((byte)numFillStyles);
        }

        for (var i = 0; i < numFillStyles; i++)
            FillStyle.EncodeMorph(body, Start.FillStyles[i], End.FillStyles[i]);

        var numLineStyles = Start.LineStyles.Length;

        if (numLineStyles >= 255)
        {
            body.WriteUInt8(255);
            body.WriteUInt16((ushort)numLineStyles);
        }
        else
        {
            body.WriteUInt8((byte)numLineStyles);
        }

        for (var i = 0; i < numLineStyles; i++)
            LineStyle.EncodeMorph(body, Start.LineStyles[i], End.LineStyles[i], 2);

        var numFillBits = 0;
        var numLineBits = 0;

        foreach (var shape in Start.Shapes)
        {
            if (shape is StyleChangeRecord styleChange)
            {
                if (styleChange.FillStyle0 is { } fillStyle0)
                    numFillBits = Math.Max(numFillBits, BitWriter.UnsignedBitsNeeded(fillStyle0));

                if (styleChange.FillStyle1 is { } fillStyle1)
                    numFillBits = Math.Max(numFillBits, BitWriter.UnsignedBitsNeeded(fillStyle1));

                if (styleChange.LineStyle is { } lineStyle)
                    numLineBits = Math.Max(numLineBits, BitWriter.UnsignedBitsNeeded(lineStyle));
            }
        }

        var bits = new BitWriter();
        bits.WriteUBits(body, (uint)numFillBits, 4);
        bits.WriteUBits(body, (uint)numLineBits, 4);

        var startContext = new ShapeContext(swfVersion, 2, numFillBits, numLineBits);

        foreach (var shape in Start.Shapes)
            shape.Encode(body, bits, startContext);

        bits.Flush(body);

        body.WriteUInt8(0);

        var endBody = new MemoryWriter();

        bits = new BitWriter();

        var endContext = new ShapeContext(swfVersion, 2, 0, 0);

        foreach (var shape in End.Shapes)
            shape.Encode(endBody, bits, endContext);

        bits.Flush(endBody);

        writer.WriteUInt32((uint)body.Position);
        writer.WriteMemory(body.WrittenMemory);
        writer.WriteMemory(endBody.WrittenMemory);
    }
}
