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
public struct Rgba :
    IFormattable,
    IEquatable<Rgba>,
    IEqualityOperators<Rgba, Rgba, bool>,
    IReadable<Rgba>
{
    public byte R;

    public byte G;

    public byte B;

    public byte A;

    public Rgba(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public bool Equals(Rgba other)
    {
        return R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rgba other && Equals(other);
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

    public static bool operator ==(Rgba left, Rgba right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rgba left, Rgba right)
    {
        return !left.Equals(right);
    }

    public static implicit operator Color(Rgba rgba)
    {
        return new Color(rgba.R, rgba.G, rgba.B, rgba.A);
    }

    public static implicit operator Rgba(Color color)
    {
        return new Rgba(color.R, color.G, color.B, color.A);
    }

    public static Rgba Read(ref SpanReader reader)
    {
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();
        var a = reader.ReadUInt8();

        return new Rgba(r, g, b, a);
    }
}
