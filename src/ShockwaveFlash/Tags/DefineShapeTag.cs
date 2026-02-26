// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags;

public record DefineShapeTag(TagMetadata Metadata, ushort ShapeId, Rectangle ShapeBounds, ShapeStyles Styles, IReadOnlyList<ShapeRecord> Shapes) : Tag(Metadata)
{
    public static DefineShapeTag Decode(ref SpanReader reader, TagMetadata metadata, byte swfVersion, byte shapeVersion)
    {
        var shapeId = reader.ReadUInt16();
        var shapeBounds = Rectangle.Decode(ref reader);
        var edgeBounds = new Rectangle();
        var flags = ShapeFlags.HasNonScalingStrokes;

        if (shapeVersion >= 4)
        {
            edgeBounds = Rectangle.Decode(ref reader);
            flags = (ShapeFlags)reader.ReadUInt8();
        }

        var (styles, numFillBits, numLineBits) = ShapeStyles.Decode(ref reader, swfVersion, shapeVersion);

        var shapeContext = new ShapeContext(swfVersion, shapeVersion, numFillBits, numLineBits);

        var records = new List<ShapeRecord>();

        var bits = new BitReader();

        while (reader.Remaining > 0)
            records.Add(ShapeRecord.Decode(ref reader, ref bits, ref shapeContext));

        return shapeVersion switch
        {
            1 => new DefineShapeTag(metadata, shapeId, shapeBounds, styles, records),
            2 => new DefineShape2Tag(metadata, shapeId, shapeBounds, styles, records),
            3 => new DefineShape3Tag(metadata, shapeId, shapeBounds, styles, records),
            4 => new DefineShape4Tag(metadata, shapeId, shapeBounds, edgeBounds, flags, styles, records),
            _ => throw new NotSupportedException($"Shape version {shapeVersion} is not supported.")
        };
    }
}
