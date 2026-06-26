namespace ShockwaveFlash.Types.Shape;

public abstract class ShapeRecord
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

                var deltaX = !isAxisAligned || !isVertical ? bits.ReadSBits(reader, nBits) : 0;
                var deltaY = !isAxisAligned || isVertical ? bits.ReadSBits(reader, nBits) : 0;

                return new StraightEdgeRecord(new Point(deltaX, deltaY));
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

    public static void EncodeCollection(MemoryWriter writer, IReadOnlyList<ShapeRecord> records, ShapeContext context)
    {
        var bits = new BitWriter();

        foreach (var record in records)
            record.Encode(writer, bits, context);

        bits.Flush(writer);
    }

    public void Encode(MemoryWriter writer, BitWriter bits, ShapeContext context)
    {
        switch (this)
        {
            case StraightEdgeRecord straight:
                {
                    var isGeneral = straight.Delta.X != 0 && straight.Delta.Y != 0;
                    var isVertical = straight.Delta.X is 0 && straight.Delta.Y != 0;

                    var nBits = isGeneral
                        ? Math.Max(BitWriter.SignedBitsNeeded(straight.Delta.X), BitWriter.SignedBitsNeeded(straight.Delta.Y))
                        : isVertical
                            ? BitWriter.SignedBitsNeeded(straight.Delta.Y)
                            : BitWriter.SignedBitsNeeded(straight.Delta.X);

                    nBits = Math.Max(nBits, 2);

                    bits.WriteBit(writer, true);
                    bits.WriteBit(writer, true);
                    bits.WriteUBits(writer, (uint)(nBits - 2), 4);

                    bits.WriteBit(writer, isGeneral);

                    if (isGeneral)
                    {
                        bits.WriteSBits(writer, straight.Delta.X, nBits);
                        bits.WriteSBits(writer, straight.Delta.Y, nBits);
                    }
                    else
                    {
                        bits.WriteBit(writer, isVertical);

                        if (isVertical)
                            bits.WriteSBits(writer, straight.Delta.Y, nBits);
                        else
                            bits.WriteSBits(writer, straight.Delta.X, nBits);
                    }

                    break;
                }

            case CurvedEdgeRecord curved:
                {
                    var nBits = Math.Max(
                        Math.Max(BitWriter.SignedBitsNeeded(curved.ControlDelta.X), BitWriter.SignedBitsNeeded(curved.ControlDelta.Y)),
                        Math.Max(BitWriter.SignedBitsNeeded(curved.AnchorDelta.X), BitWriter.SignedBitsNeeded(curved.AnchorDelta.Y)));

                    nBits = Math.Max(nBits, 2);

                    bits.WriteBit(writer, true);
                    bits.WriteBit(writer, false);
                    bits.WriteUBits(writer, (uint)(nBits - 2), 4);

                    bits.WriteSBits(writer, curved.ControlDelta.X, nBits);
                    bits.WriteSBits(writer, curved.ControlDelta.Y, nBits);
                    bits.WriteSBits(writer, curved.AnchorDelta.X, nBits);
                    bits.WriteSBits(writer, curved.AnchorDelta.Y, nBits);

                    break;
                }

            case EndShapeRecord:
                {
                    bits.WriteBit(writer, false);
                    bits.WriteUBits(writer, (uint)ShapeRecordFlags.None, 5);

                    break;
                }

            case StyleChangeRecord styleChange:
                {
                    var flags = ShapeRecordFlags.None;

                    if (styleChange.MoveTo is not null)
                        flags |= ShapeRecordFlags.MoveTo;

                    if (styleChange.FillStyle0 is not null)
                        flags |= ShapeRecordFlags.FillStyle0;

                    if (styleChange.FillStyle1 is not null)
                        flags |= ShapeRecordFlags.FillStyle1;

                    if (styleChange.LineStyle is not null)
                        flags |= ShapeRecordFlags.LineStyle;

                    if (styleChange.NewStyles is not null)
                        flags |= ShapeRecordFlags.NewStyles;

                    bits.WriteBit(writer, false);
                    bits.WriteUBits(writer, (uint)flags, 5);

                    if (styleChange.MoveTo is { } moveTo)
                    {
                        var nBits = Math.Max(BitWriter.SignedBitsNeeded(moveTo.X), BitWriter.SignedBitsNeeded(moveTo.Y));
                        bits.WriteUBits(writer, (uint)nBits, 5);
                        bits.WriteSBits(writer, moveTo.X, nBits);
                        bits.WriteSBits(writer, moveTo.Y, nBits);
                    }

                    if (styleChange.FillStyle0 is { } fillStyle0)
                        bits.WriteUBits(writer, fillStyle0, context.NumFillBits);

                    if (styleChange.FillStyle1 is { } fillStyle1)
                        bits.WriteUBits(writer, fillStyle1, context.NumFillBits);

                    if (styleChange.LineStyle is { } lineStyle)
                        bits.WriteUBits(writer, lineStyle, context.NumLineBits);

                    if (styleChange.NewStyles is { } newStyles)
                    {
                        bits.Flush(writer);

                        var (newNumFillBits, newNumLineBits) = newStyles.Encode(writer, context.SwfVersion, context.ShapeVersion);

                        bits.Reset();
                        context.NumFillBits = newNumFillBits;
                        context.NumLineBits = newNumLineBits;
                    }

                    break;
                }

            default:
                throw new NotSupportedException($"Shape record {this} is not supported.");
        }
    }
}
