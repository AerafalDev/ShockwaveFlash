// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

public abstract record ShapeRecord
{
    public static ShapeRecord Decode(ref SpanReader reader, ref BitReader bits, ref ShapeContext context)
    {
        var isEdgeRecord = bits.ReadBit(ref reader);

        if (isEdgeRecord)
        {
            var isStraightEdge = bits.ReadBit(ref reader);
            var nBits = bits.ReadIBits(ref reader, 4) + 2;

            if (isStraightEdge)
            {
                var isAxisAligned = !bits.ReadBit(ref reader);
                var isVertical = isAxisAligned && bits.ReadBit(ref reader);
                var delta = Point.Zero;

                if (!isAxisAligned || !isVertical)
                    delta.X = bits.ReadSBits(ref reader, nBits);

                if (!isAxisAligned || isVertical)
                    delta.Y = bits.ReadSBits(ref reader, nBits);

                return new StraightEdgeRecord(delta);
            }

            var controlDelta = new Point(bits.ReadSBits(ref reader, nBits), bits.ReadSBits(ref reader, nBits));
            var anchorDelta = new Point(bits.ReadSBits(ref reader, nBits), bits.ReadSBits(ref reader, nBits));

            return new CurvedEdgeRecord(controlDelta, anchorDelta);
        }

        var flags = (ShapeRecordFlags)bits.ReadIBits(ref reader, 5);

        if (flags is ShapeRecordFlags.None)
            return new EndShapeRecord();

        var numFillBits = context.NumFillBits;
        var numLineBits = context.NumLineBits;

        Point? moveTo = null;

        if (flags.HasFlag(ShapeRecordFlags.MoveTo))
        {
            var nBits = bits.ReadIBits(ref reader, 5);
            var moveToX = bits.ReadSBits(ref reader, nBits);
            var moveToY = bits.ReadSBits(ref reader, nBits);
            moveTo = new Point(moveToX, moveToY);
        }

        uint? fillStyle0 = null;

        if (flags.HasFlag(ShapeRecordFlags.FillStyle0))
            fillStyle0 = bits.ReadUBits(ref reader, numFillBits);

        uint? fillStyle1 = null;

        if (flags.HasFlag(ShapeRecordFlags.FillStyle1))
            fillStyle1 = bits.ReadUBits(ref reader, numFillBits);

        uint? lineStyle = null;

        if (flags.HasFlag(ShapeRecordFlags.LineStyle))
            lineStyle = bits.ReadUBits(ref reader, numLineBits);

        ShapeStyles? styles = null;

        if (flags.HasFlag(ShapeRecordFlags.NewStyles))
        {
            var (newStyles, newNumFillBits, newNumLineBits) = ShapeStyles.Decode(ref reader, context.SwfVersion, context.ShapeVersion);

            bits = new BitReader();
            context = context with { NumFillBits = newNumFillBits, NumLineBits = newNumLineBits };

            styles = newStyles;
        }

        return new StyleChangeRecord(moveTo, fillStyle0, fillStyle1, lineStyle, styles);
    }
}
