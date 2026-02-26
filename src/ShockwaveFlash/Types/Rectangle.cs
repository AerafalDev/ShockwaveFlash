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
public struct Rectangle :
    IFormattable,
    IEquatable<Rectangle>,
    IEqualityOperators<Rectangle, Rectangle, bool>
{
    public int XMin;

    public int XMax;

    public int YMin;

    public int YMax;

    public int Width =>
        Math.Max(XMax - XMin, 0);

    public int Height =>
        Math.Max(YMax - YMin, 0);

    public Point TopLeft =>
        new(XMin, YMin);

    public Point BottomRight =>
        new(XMax, YMax);

    public Point TopRight =>
        new(XMax, YMin);

    public Point BottomLeft =>
        new(XMin, YMax);

    public Point Center =>
        new(XMin + Width / 2, YMin + Height / 2);

    public Rectangle(int xMin, int xMax, int yMin, int yMax)
    {
        XMin = xMin;
        XMax = xMax;
        YMin = yMin;
        YMax = yMax;
    }

    public bool Equals(Rectangle other)
    {
        return XMin == other.XMin && XMax == other.XMax && YMin == other.YMin && YMax == other.YMax;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rectangle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(XMin, XMax, YMin, YMax);
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        return $"{XMin.ToString(format, formatProvider)}..{XMax.ToString(format, formatProvider)}, {YMin.ToString(format, formatProvider)}..{YMax.ToString(format, formatProvider)}";
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public static bool operator ==(Rectangle left, Rectangle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rectangle left, Rectangle right)
    {
        return !left.Equals(right);
    }

    internal static Rectangle Decode(ref SpanReader reader)
    {
        var rectangle = new Rectangle();

        var bits = new BitReader();

        var nBits = bits.ReadIBits(ref reader, 5);

        rectangle.XMin = bits.ReadSBits(ref reader, nBits);
        rectangle.XMax = bits.ReadSBits(ref reader, nBits);
        rectangle.YMin = bits.ReadSBits(ref reader, nBits);
        rectangle.YMax = bits.ReadSBits(ref reader, nBits);

        return rectangle;
    }
}
