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
        return $"Rectangle(XMin: {XMin.ToString(format, formatProvider)}, XMax: {XMax.ToString(format, formatProvider)}, YMin: {YMin.ToString(format, formatProvider)}, YMax: {YMax.ToString(format, formatProvider)})";
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

    internal static Rectangle Decode(MemoryReader reader)
    {
        var rectangle = new Rectangle();

        var bits = new BitReader();

        var nBits = bits.ReadIBits(reader, 5);

        rectangle.XMin = bits.ReadSBits(reader, nBits);
        rectangle.XMax = bits.ReadSBits(reader, nBits);
        rectangle.YMin = bits.ReadSBits(reader, nBits);
        rectangle.YMax = bits.ReadSBits(reader, nBits);

        return rectangle;
    }

    internal readonly void Encode(MemoryWriter writer)
    {
        var bits = new BitWriter();

        var nBits = Math.Max(
            Math.Max(BitWriter.SignedBitsNeeded(XMin), BitWriter.SignedBitsNeeded(XMax)),
            Math.Max(BitWriter.SignedBitsNeeded(YMin), BitWriter.SignedBitsNeeded(YMax)));

        bits.WriteUBits(writer, (uint)nBits, 5);
        bits.WriteSBits(writer, XMin, nBits);
        bits.WriteSBits(writer, XMax, nBits);
        bits.WriteSBits(writer, YMin, nBits);
        bits.WriteSBits(writer, YMax, nBits);
        bits.Flush(writer);
    }
}
