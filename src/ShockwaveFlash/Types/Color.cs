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
public struct Color :
    IFormattable,
    IEquatable<Color>,
    IEqualityOperators<Color, Color, bool>
{
    public static Color Transparent =>
        new(0, 0, 0, 0);

    public static Color Black =>
        new(0, 0, 0, 255);

    public static Color White =>
        new(255, 255, 255, 255);

    public byte R;

    public byte G;

    public byte B;

    public byte A;

    public Color(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(float r, float g, float b, float a)
    {
        R = (byte)(r * 255);
        G = (byte)(g * 255);
        B = (byte)(b * 255);
        A = (byte)(a * 255);
    }

    public Color(int color)
    {
        A = (byte)((color >> 24) & 0xFF);
        R = (byte)((color >> 16) & 0xFF);
        G = (byte)((color >> 8) & 0xFF);
        B = (byte)(color & 0xFF);
    }

    public (float R, float G, float B, float A) ToFloat()
    {
        return (R / 255f, G / 255f, B / 255f, A / 255f);
    }

    public int ToInt()
    {
        return A << 24 | R << 16 | G << 8 | B;
    }

    public string ToHexArgb()
    {
        return $"#{A:X2}{R:X2}{G:X2}{B:X2}";
    }

    public string ToHexRgba()
    {
        return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }

    public string ToHexRgb()
    {
        return $"#{R:X2}{G:X2}{B:X2}";
    }

    public bool Equals(Color other)
    {
        return R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public override bool Equals(object? obj)
    {
        return obj is Color other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        return $"Color(R: {R.ToString(format, formatProvider)}, G: {G.ToString(format, formatProvider)}, B: {B.ToString(format, formatProvider)}, A: {A.ToString(format, formatProvider)})";
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }

    internal static Color DecodeArgb(ref SpanReader reader)
    {
        var a = reader.ReadUInt8();
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();

        return new Color(r, g, b, a);
    }

    internal static Color DecodeRgba(ref SpanReader reader)
    {
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();
        var a = reader.ReadUInt8();

        return new Color(r, g, b, a);
    }

    internal static Color DecodeRgb(ref SpanReader reader)
    {
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();

        return new Color(r, g, b, 255);
    }
}
