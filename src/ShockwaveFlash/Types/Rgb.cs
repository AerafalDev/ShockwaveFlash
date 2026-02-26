// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using ShockwaveFlash.IO.Binary;
using ShockwaveFlash.Types.Abstractions;

namespace ShockwaveFlash.Types;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public struct Rgb :
    IFormattable,
    IEquatable<Rgb>,
    IEqualityOperators<Rgb, Rgb, bool>,
    IReadable<Rgb>
{
    public byte R;

    public byte G;

    public byte B;

    public Rgb(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public bool Equals(Rgb other)
    {
        return R == other.R && G == other.G && B == other.B;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rgb other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B);
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        return $"RGB({R.ToString(format, formatProvider)}, {G.ToString(format, formatProvider)}, {B.ToString(format, formatProvider)})";
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public static bool operator ==(Rgb left, Rgb right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rgb left, Rgb right)
    {
        return !left.Equals(right);
    }

    public static implicit operator Color(Rgb rgb)
    {
        return new Color(rgb.R, rgb.G, rgb.B, 255);
    }

    public static implicit operator Rgb(Color color)
    {
        return new Rgb(color.R, color.G, color.B);
    }

    public static Rgb Read(ref SpanReader reader)
    {
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();

        return new Rgb(r, g, b);
    }
}
