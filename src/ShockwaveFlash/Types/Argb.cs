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
public struct Argb :
    IFormattable,
    IEquatable<Argb>,
    IEqualityOperators<Argb, Argb, bool>,
    IReadable<Argb>
{
    public byte A;

    public byte R;

    public byte G;

    public byte B;

    public Argb(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public bool Equals(Argb other)
    {
        return A == other.A && R == other.R && G == other.G && B == other.B;
    }

    public override bool Equals(object? obj)
    {
        return obj is Argb other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(A, R, G, B);
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        return $"ARGB({A.ToString(format, formatProvider)}, {R.ToString(format, formatProvider)}, {G.ToString(format, formatProvider)}, {B.ToString(format, formatProvider)})";
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public static bool operator ==(Argb left, Argb right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Argb left, Argb right)
    {
        return !left.Equals(right);
    }

    public static implicit operator Color(Argb argb)
    {
        return new Color(argb.R, argb.G, argb.B, argb.A);
    }

    public static implicit operator Argb(Color color)
    {
        return new Argb(color.A, color.R, color.G, color.B);
    }

    public static Argb Read(ref SpanReader reader)
    {
        var a = reader.ReadUInt8();
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();

        return new Argb(a, r, g, b);
    }
}
