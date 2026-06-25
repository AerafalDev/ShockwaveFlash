// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
}
