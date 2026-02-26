// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
        var color = new Color();

        color.R = (byte)Math.Clamp(other.R * RMult / 256 + RAdd, 0, 255);
        color.G = (byte)Math.Clamp(other.G * GMult / 256 + GAdd, 0, 255);
        color.B = (byte)Math.Clamp(other.B * BMult / 256 + BAdd, 0, 255);
        color.A = (byte)Math.Clamp(other.A * AMult / 256 + AAdd, 0, 255);

        return color;
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

    public static ColorTransform DecodeRgba(ref SpanReader reader)
    {
        var colorTransform = new ColorTransform();

        var bits = new BitReader();

        var hasAddTerms = bits.ReadBit(ref reader);
        var hasMultTerms = bits.ReadBit(ref reader);

        var nBits = bits.ReadIBits(ref reader, 4);

        if (hasMultTerms)
        {
            colorTransform.RMult = bits.ReadSBits(ref reader, nBits);
            colorTransform.GMult = bits.ReadSBits(ref reader, nBits);
            colorTransform.BMult = bits.ReadSBits(ref reader, nBits);
            colorTransform.AMult = bits.ReadSBits(ref reader, nBits);
        }

        if (hasAddTerms)
        {
            colorTransform.RAdd = bits.ReadSBits(ref reader, nBits);
            colorTransform.GAdd = bits.ReadSBits(ref reader, nBits);
            colorTransform.BAdd = bits.ReadSBits(ref reader, nBits);
            colorTransform.AAdd = bits.ReadSBits(ref reader, nBits);
        }

        return colorTransform;
    }

    public static ColorTransform DecodeRgb(ref SpanReader reader)
    {
        var colorTransform = new ColorTransform();

        var bits = new BitReader();

        var hasAddTerms = bits.ReadBit(ref reader);
        var hasMultTerms = bits.ReadBit(ref reader);

        var nBits = bits.ReadIBits(ref reader, 4);

        if (hasMultTerms)
        {
            colorTransform.RMult = bits.ReadSBits(ref reader, nBits);
            colorTransform.GMult = bits.ReadSBits(ref reader, nBits);
            colorTransform.BMult = bits.ReadSBits(ref reader, nBits);
        }

        if (hasAddTerms)
        {
            colorTransform.RAdd = bits.ReadSBits(ref reader, nBits);
            colorTransform.GAdd = bits.ReadSBits(ref reader, nBits);
            colorTransform.BAdd = bits.ReadSBits(ref reader, nBits);
        }

        return colorTransform;
    }
}
