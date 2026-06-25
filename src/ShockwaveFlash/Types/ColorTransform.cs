using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ShockwaveFlash.Types;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public struct ColorTransform :
    IFormattable,
    IEquatable<ColorTransform>,
    IEqualityOperators<ColorTransform, ColorTransform, bool>
{
    public static ColorTransform Identity =>
        new(256, 256, 256, 256, 0, 0, 0, 0);

    public int RMult;

    public int GMult;

    public int BMult;

    public int AMult;

    public int RAdd;

    public int GAdd;

    public int BAdd;

    public int AAdd;

    public ColorTransform()
    {
        this = Identity;
    }

    public ColorTransform(int rMult, int gMult, int bMult, int aMult, int rAdd, int gAdd, int bAdd, int aAdd)
    {
        RMult = rMult;
        GMult = gMult;
        BMult = bMult;
        AMult = aMult;
        RAdd = rAdd;
        GAdd = gAdd;
        BAdd = bAdd;
        AAdd = aAdd;
    }

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
        var colorTransform = new ColorTransform();

        colorTransform.RMult = (RMult * other.RMult) >> 8;
        colorTransform.GMult = (GMult * other.GMult) >> 8;
        colorTransform.BMult = (BMult * other.BMult) >> 8;
        colorTransform.AMult = (AMult * other.AMult) >> 8;

        colorTransform.RAdd = ((RAdd * other.RMult) >> 8) + other.RAdd;
        colorTransform.GAdd = ((GAdd * other.GMult) >> 8) + other.GAdd;
        colorTransform.BAdd = ((BAdd * other.BMult) >> 8) + other.BAdd;
        colorTransform.AAdd = ((AAdd * other.AMult) >> 8) + other.AAdd;

        return colorTransform;
    }

    public bool Equals(ColorTransform other)
    {
        return RMult == other.RMult &&
               GMult == other.GMult &&
               BMult == other.BMult &&
               AMult == other.AMult &&
               RAdd == other.RAdd &&
               GAdd == other.GAdd &&
               BAdd == other.BAdd &&
               AAdd == other.AAdd;
    }

    public override bool Equals(object? obj)
    {
        return obj is ColorTransform other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RMult, GMult, BMult, AMult, RAdd, GAdd, BAdd, AAdd);
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        return $"ColorTransform(RMult: {RMult.ToString(format, formatProvider)}, GMult: {GMult.ToString(format, formatProvider)}, BMult: {BMult.ToString(format, formatProvider)}, AMult: {AMult.ToString(format, formatProvider)}, RAdd: {RAdd.ToString(format, formatProvider)}, GAdd: {GAdd.ToString(format, formatProvider)}, BAdd: {BAdd.ToString(format, formatProvider)}, AAdd: {AAdd.ToString(format, formatProvider)})";
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public static bool operator ==(ColorTransform left, ColorTransform right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ColorTransform left, ColorTransform right)
    {
        return !left.Equals(right);
    }

    public static ColorTransform DecodeRgba(MemoryReader reader)
    {
        var colorTransform = new ColorTransform();

        var bits = new BitReader();

        var hasAddTerms = bits.ReadBit(reader);
        var hasMultTerms = bits.ReadBit(reader);

        var nBits = bits.ReadIBits(reader, 4);

        if (hasMultTerms)
        {
            colorTransform.RMult = bits.ReadSBits(reader, nBits);
            colorTransform.GMult = bits.ReadSBits(reader, nBits);
            colorTransform.BMult = bits.ReadSBits(reader, nBits);
            colorTransform.AMult = bits.ReadSBits(reader, nBits);
        }

        if (hasAddTerms)
        {
            colorTransform.RAdd = bits.ReadSBits(reader, nBits);
            colorTransform.GAdd = bits.ReadSBits(reader, nBits);
            colorTransform.BAdd = bits.ReadSBits(reader, nBits);
            colorTransform.AAdd = bits.ReadSBits(reader, nBits);
        }

        return colorTransform;
    }

    public static ColorTransform DecodeRgb(MemoryReader reader)
    {
        var colorTransform = new ColorTransform();

        var bits = new BitReader();

        var hasAddTerms = bits.ReadBit(reader);
        var hasMultTerms = bits.ReadBit(reader);

        var nBits = bits.ReadIBits(reader, 4);

        if (hasMultTerms)
        {
            colorTransform.RMult = bits.ReadSBits(reader, nBits);
            colorTransform.GMult = bits.ReadSBits(reader, nBits);
            colorTransform.BMult = bits.ReadSBits(reader, nBits);
        }

        if (hasAddTerms)
        {
            colorTransform.RAdd = bits.ReadSBits(reader, nBits);
            colorTransform.GAdd = bits.ReadSBits(reader, nBits);
            colorTransform.BAdd = bits.ReadSBits(reader, nBits);
        }

        return colorTransform;
    }

    public readonly void EncodeRgba(MemoryWriter writer)
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

    public readonly void EncodeRgb(MemoryWriter writer)
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
