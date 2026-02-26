// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ShockwaveFlash.Tags;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public readonly struct TagMetadata :
    IFormattable,
    IEquatable<TagMetadata>,
    IEqualityOperators<TagMetadata, TagMetadata, bool>
{
    public readonly TagCode Code;

    public readonly uint Offset;

    public readonly uint Length;

    public TagMetadata(TagCode code, uint offset, uint length)
    {
        Code = code;
        Offset = offset;
        Length = length;
    }

    public bool Equals(TagMetadata other)
    {
        return Code == other.Code && Offset == other.Offset && Length == other.Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is TagMetadata other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Offset, Length);
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        return $"TagMetadata(Code: {Code}, Offset: {Offset.ToString(format, formatProvider)}, Length: {Length.ToString(format, formatProvider)})";
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public static bool operator ==(TagMetadata left, TagMetadata right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TagMetadata left, TagMetadata right)
    {
        return !left.Equals(right);
    }
}
