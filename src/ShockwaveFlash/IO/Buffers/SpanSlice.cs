// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ShockwaveFlash.IO.Buffers;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public readonly struct SpanSlice :
    IFormattable,
    IEquatable<SpanSlice>,
    IEqualityOperators<SpanSlice, SpanSlice, bool>
{
    public readonly int Offset;

    public readonly int Length;

    public SpanSlice(int offset, int length)
    {
        Offset = offset;
        Length = length;
    }

    public bool Equals(SpanSlice other)
    {
        return Offset == other.Offset && Length == other.Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is SpanSlice other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Offset, Length);
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        return $"{Offset.ToString(format, formatProvider)}..{(Offset + Length).ToString(format, formatProvider)}";
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public static bool operator ==(SpanSlice left, SpanSlice right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SpanSlice left, SpanSlice right)
    {
        return !left.Equals(right);
    }
}
