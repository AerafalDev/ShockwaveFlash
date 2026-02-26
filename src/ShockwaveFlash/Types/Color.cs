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
        return $"RGBA({R.ToString(format, formatProvider)}, {G.ToString(format, formatProvider)}, {B.ToString(format, formatProvider)}, {A.ToString(format, formatProvider)})";
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

    public static implicit operator Color(Argb argb)
    {
        return new Color(argb.R, argb.G, argb.B, argb.A);
    }

    public static implicit operator Color(Rgba rgba)
    {
        return new Color(rgba.R, rgba.G, rgba.B, rgba.A);
    }

    public static implicit operator Color(Rgb rgb)
    {
        return new Color(rgb.R, rgb.G, rgb.B, 255);
    }

    public static implicit operator Argb(Color color)
    {
        return new Argb(color.A, color.R, color.G, color.B);
    }

    public static implicit operator Rgba(Color color)
    {
        return new Rgba(color.R, color.G, color.B, color.A);
    }

    public static implicit operator Rgb(Color color)
    {
        return new Rgb(color.R, color.G, color.B);
    }
}
