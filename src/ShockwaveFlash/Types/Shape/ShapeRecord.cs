// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

public abstract record ShapeRecord
{
    public static ShapeRecord Decode(MemoryReader reader, BitReader bits, ShapeContext context)
    {
        var isEdgeRecord = bits.ReadBit(reader);

        if (isEdgeRecord)
        {
            var isStraightEdge = bits.ReadBit(reader);
            var nBits = bits.ReadIBits(reader, 4) + 2;

            if (isStraightEdge)
            {
                var isAxisAligned = !bits.ReadBit(reader);
                var isVertical = isAxisAligned && bits.ReadBit(reader);
                var delta = Point.Zero;

                if (!isAxisAligned || !isVertical)
                    delta.X = bits.ReadSBits(reader, nBits);

                if (!isAxisAligned || isVertical)
                    delta.Y = bits.ReadSBits(reader, nBits);

                return new StraightEdgeRecord(delta);
            }

            var controlDelta = new Point(bits.ReadSBits(reader, nBits), bits.ReadSBits(reader, nBits));
            var anchorDelta = new Point(bits.ReadSBits(reader, nBits), bits.ReadSBits(reader, nBits));

            return new CurvedEdgeRecord(controlDelta, anchorDelta);
        }

        var flags = (ShapeRecordFlags)bits.ReadIBits(reader, 5);

        if (flags is ShapeRecordFlags.None)
            return new EndShapeRecord();

        var numFillBits = context.NumFillBits;
        var numLineBits = context.NumLineBits;

        Point? moveTo = null;

        if (flags.HasFlag(ShapeRecordFlags.MoveTo))
        {
            var nBits = bits.ReadIBits(reader, 5);
            var moveToX = bits.ReadSBits(reader, nBits);
            var moveToY = bits.ReadSBits(reader, nBits);
            moveTo = new Point(moveToX, moveToY);
        }

        uint? fillStyle0 = null;

        if (flags.HasFlag(ShapeRecordFlags.FillStyle0))
            fillStyle0 = bits.ReadUBits(reader, numFillBits);

        uint? fillStyle1 = null;

        if (flags.HasFlag(ShapeRecordFlags.FillStyle1))
            fillStyle1 = bits.ReadUBits(reader, numFillBits);

        uint? lineStyle = null;

        if (flags.HasFlag(ShapeRecordFlags.LineStyle))
            lineStyle = bits.ReadUBits(reader, numLineBits);

        ShapeStyles? styles = null;

        if (flags.HasFlag(ShapeRecordFlags.NewStyles))
        {
            var (newStyles, newNumFillBits, newNumLineBits) = ShapeStyles.Decode(reader, context.SwfVersion, context.ShapeVersion);

            bits.Reset();
            context.NumFillBits = newNumFillBits;
            context.NumLineBits = newNumLineBits;

            styles = newStyles;
        }

        return new StyleChangeRecord(moveTo, fillStyle0, fillStyle1, lineStyle, styles);
    }
}
