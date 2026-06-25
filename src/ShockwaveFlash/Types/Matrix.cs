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

    public Matrix()
    {
        this = Identity;
    }

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
        return $"Matrix(Translation: {Translation.ToString(format, formatProvider)}, Scale: {Scale.ToString(format, formatProvider)}, Rotation: {Rotation.ToString(format, formatProvider)})";
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

    public static Matrix Decode(MemoryReader reader)
    {
        var matrix = new Matrix();

        var bits = new BitReader();

        var hasScale = bits.ReadBit(reader);

        if (hasScale)
        {
            var nScaleBits = bits.ReadIBits(reader, 5);
            matrix.Scale.X = bits.ReadFBits(reader, nScaleBits);
            matrix.Scale.Y = bits.ReadFBits(reader, nScaleBits);
        }

        var hasRotation = bits.ReadBit(reader);

        if (hasRotation)
        {
            var nRotationBits = bits.ReadIBits(reader, 5);
            matrix.Rotation.X = bits.ReadFBits(reader, nRotationBits);
            matrix.Rotation.Y = bits.ReadFBits(reader, nRotationBits);
        }

        var nTranslationBits = bits.ReadIBits(reader, 5);

        matrix.Translation.X = bits.ReadSBits(reader, nTranslationBits);
        matrix.Translation.Y = bits.ReadSBits(reader, nTranslationBits);

        return matrix;
    }

    public readonly void Encode(MemoryWriter writer)
    {
        var bits = new BitWriter();

        var hasScale = Scale.X != 1f || Scale.Y != 1f;

        bits.WriteBit(writer, hasScale);

        if (hasScale)
        {
            var nScaleBits = Math.Max(FixedBitsNeeded(Scale.X), FixedBitsNeeded(Scale.Y));
            bits.WriteUBits(writer, (uint)nScaleBits, 5);
            bits.WriteFBits(writer, Scale.X, nScaleBits);
            bits.WriteFBits(writer, Scale.Y, nScaleBits);
        }

        var hasRotation = Rotation.X != 0f || Rotation.Y != 0f;

        bits.WriteBit(writer, hasRotation);

        if (hasRotation)
        {
            var nRotationBits = Math.Max(FixedBitsNeeded(Rotation.X), FixedBitsNeeded(Rotation.Y));
            bits.WriteUBits(writer, (uint)nRotationBits, 5);
            bits.WriteFBits(writer, Rotation.X, nRotationBits);
            bits.WriteFBits(writer, Rotation.Y, nRotationBits);
        }

        var nTranslationBits = Math.Max(BitWriter.SignedBitsNeeded(Translation.X), BitWriter.SignedBitsNeeded(Translation.Y));

        bits.WriteUBits(writer, (uint)nTranslationBits, 5);
        bits.WriteSBits(writer, Translation.X, nTranslationBits);
        bits.WriteSBits(writer, Translation.Y, nTranslationBits);

        bits.Flush(writer);
    }

    private static int FixedBitsNeeded(float value)
    {
        return BitWriter.SignedBitsNeeded((int)Math.Round(value * 65536.0));
    }
}
