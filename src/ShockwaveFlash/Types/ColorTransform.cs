namespace ShockwaveFlash.Types;

public readonly record struct ColorTransform(
    int RMult,
    int GMult,
    int BMult,
    int AMult,
    int RAdd,
    int GAdd,
    int BAdd,
    int AAdd)
{
    public static ColorTransform Identity =>
        new(256, 256, 256, 256, 0, 0, 0, 0);

    public Color Transform(Color other)
    {
        return new Color(
            (byte)Math.Clamp(other.R * RMult / 256 + RAdd, 0, 255),
            (byte)Math.Clamp(other.G * GMult / 256 + GAdd, 0, 255),
            (byte)Math.Clamp(other.B * BMult / 256 + BAdd, 0, 255),
            (byte)Math.Clamp(other.A * AMult / 256 + AAdd, 0, 255));
    }

    public ColorTransform Merge(ColorTransform other)
    {
        return new ColorTransform(
            (RMult * other.RMult) >> 8,
            (GMult * other.GMult) >> 8,
            (BMult * other.BMult) >> 8,
            (AMult * other.AMult) >> 8,
            ((RAdd * other.RMult) >> 8) + other.RAdd,
            ((GAdd * other.GMult) >> 8) + other.GAdd,
            ((BAdd * other.BMult) >> 8) + other.BAdd,
            ((AAdd * other.AMult) >> 8) + other.AAdd);
    }

    public static ColorTransform DecodeRgba(MemoryReader reader)
    {
        var bits = new BitReader();

        var hasAddTerms = bits.ReadBit(reader);
        var hasMultTerms = bits.ReadBit(reader);

        var nBits = bits.ReadIBits(reader, 4);

        int rMult = 256, gMult = 256, bMult = 256, aMult = 256;
        int rAdd = 0, gAdd = 0, bAdd = 0, aAdd = 0;

        if (hasMultTerms)
        {
            rMult = bits.ReadSBits(reader, nBits);
            gMult = bits.ReadSBits(reader, nBits);
            bMult = bits.ReadSBits(reader, nBits);
            aMult = bits.ReadSBits(reader, nBits);
        }

        if (hasAddTerms)
        {
            rAdd = bits.ReadSBits(reader, nBits);
            gAdd = bits.ReadSBits(reader, nBits);
            bAdd = bits.ReadSBits(reader, nBits);
            aAdd = bits.ReadSBits(reader, nBits);
        }

        return new ColorTransform(rMult, gMult, bMult, aMult, rAdd, gAdd, bAdd, aAdd);
    }

    public static ColorTransform DecodeRgb(MemoryReader reader)
    {
        var bits = new BitReader();

        var hasAddTerms = bits.ReadBit(reader);
        var hasMultTerms = bits.ReadBit(reader);

        var nBits = bits.ReadIBits(reader, 4);

        int rMult = 256, gMult = 256, bMult = 256;
        int rAdd = 0, gAdd = 0, bAdd = 0;

        if (hasMultTerms)
        {
            rMult = bits.ReadSBits(reader, nBits);
            gMult = bits.ReadSBits(reader, nBits);
            bMult = bits.ReadSBits(reader, nBits);
        }

        if (hasAddTerms)
        {
            rAdd = bits.ReadSBits(reader, nBits);
            gAdd = bits.ReadSBits(reader, nBits);
            bAdd = bits.ReadSBits(reader, nBits);
        }

        return new ColorTransform(rMult, gMult, bMult, 256, rAdd, gAdd, bAdd, 0);
    }

    public void EncodeRgba(MemoryWriter writer)
    {
        var bits = new BitWriter();

        var hasMultTerms = RMult != 256 || GMult != 256 || BMult != 256 || AMult != 256;
        var hasAddTerms = RAdd != 0 || GAdd != 0 || BAdd != 0 || AAdd != 0;

        var nBits = 1;

        if (hasMultTerms)
            nBits = Math.Max(nBits, Math.Max(
                Math.Max(BitWriter.SignedBitsNeeded(RMult), BitWriter.SignedBitsNeeded(GMult)),
                Math.Max(BitWriter.SignedBitsNeeded(BMult), BitWriter.SignedBitsNeeded(AMult))));

        if (hasAddTerms)
            nBits = Math.Max(nBits, Math.Max(
                Math.Max(BitWriter.SignedBitsNeeded(RAdd), BitWriter.SignedBitsNeeded(GAdd)),
                Math.Max(BitWriter.SignedBitsNeeded(BAdd), BitWriter.SignedBitsNeeded(AAdd))));

        bits.WriteBit(writer, hasAddTerms);
        bits.WriteBit(writer, hasMultTerms);
        bits.WriteUBits(writer, (uint)nBits, 4);

        if (hasMultTerms)
        {
            bits.WriteSBits(writer, RMult, nBits);
            bits.WriteSBits(writer, GMult, nBits);
            bits.WriteSBits(writer, BMult, nBits);
            bits.WriteSBits(writer, AMult, nBits);
        }

        if (hasAddTerms)
        {
            bits.WriteSBits(writer, RAdd, nBits);
            bits.WriteSBits(writer, GAdd, nBits);
            bits.WriteSBits(writer, BAdd, nBits);
            bits.WriteSBits(writer, AAdd, nBits);
        }

        bits.Flush(writer);
    }

    public void EncodeRgb(MemoryWriter writer)
    {
        var bits = new BitWriter();

        var hasMultTerms = RMult != 256 || GMult != 256 || BMult != 256;
        var hasAddTerms = RAdd != 0 || GAdd != 0 || BAdd != 0;

        var nBits = 1;

        if (hasMultTerms)
            nBits = Math.Max(nBits, Math.Max(
                BitWriter.SignedBitsNeeded(RMult),
                Math.Max(BitWriter.SignedBitsNeeded(GMult), BitWriter.SignedBitsNeeded(BMult))));

        if (hasAddTerms)
            nBits = Math.Max(nBits, Math.Max(
                BitWriter.SignedBitsNeeded(RAdd),
                Math.Max(BitWriter.SignedBitsNeeded(GAdd), BitWriter.SignedBitsNeeded(BAdd))));

        bits.WriteBit(writer, hasAddTerms);
        bits.WriteBit(writer, hasMultTerms);
        bits.WriteUBits(writer, (uint)nBits, 4);

        if (hasMultTerms)
        {
            bits.WriteSBits(writer, RMult, nBits);
            bits.WriteSBits(writer, GMult, nBits);
            bits.WriteSBits(writer, BMult, nBits);
        }

        if (hasAddTerms)
        {
            bits.WriteSBits(writer, RAdd, nBits);
            bits.WriteSBits(writer, GAdd, nBits);
            bits.WriteSBits(writer, BAdd, nBits);
        }

        bits.Flush(writer);
    }
}
