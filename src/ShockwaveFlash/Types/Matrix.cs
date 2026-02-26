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
public struct Matrix :
    IFormattable,
    IEquatable<Matrix>,
    IEqualityOperators<Matrix, Matrix, bool>
{
    public static Matrix Identity =>
        new(Vector2.One, Vector2.Zero, Point.Zero);

    public Vector2 Scale;

    public Vector2 Rotation;

    public Point Translation;

    public Matrix(Vector2 scale, Vector2 rotation, Point translation)
    {
        Scale = scale;
        Rotation = rotation;
        Translation = translation;
    }

    public bool Equals(Matrix other)
    {
        return Scale == other.Scale && Rotation == other.Rotation && Translation == other.Translation;
    }

    public override bool Equals(object? obj)
    {
        return obj is Matrix other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Scale, Rotation, Translation);
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        return $"{Translation.ToString(format, formatProvider)}, {Scale.ToString(format, formatProvider)}, {Rotation.ToString(format, formatProvider)}";
    }

    public string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public static bool operator ==(Matrix left, Matrix right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Matrix left, Matrix right)
    {
        return !left.Equals(right);
    }

    public static Matrix Read(ref SpanReader reader)
    {
        var matrix = Identity;

        var bits = new BitReader();

        var hasScale = bits.ReadBit(ref reader);

        if (hasScale)
        {
            var nScaleBits = bits.ReadIBits(ref reader, 5);
            matrix.Scale.X = bits.ReadFBits(ref reader, nScaleBits);
            matrix.Scale.Y = bits.ReadFBits(ref reader, nScaleBits);
        }

        var hasRotation = bits.ReadBit(ref reader);

        if (hasRotation)
        {
            var nRotationBits = bits.ReadIBits(ref reader, 5);
            matrix.Rotation.X = bits.ReadFBits(ref reader, nRotationBits);
            matrix.Rotation.Y = bits.ReadFBits(ref reader, nRotationBits);
        }

        var nTranslationBits = bits.ReadIBits(ref reader, 5);

        matrix.Translation.X = bits.ReadSBits(ref reader, nTranslationBits);
        matrix.Translation.Y = bits.ReadSBits(ref reader, nTranslationBits);

        return matrix;
    }
}
