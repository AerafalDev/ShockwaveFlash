// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using ShockwaveFlash.Types.Abstractions;

namespace ShockwaveFlash.Types;

[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential)]
public struct Matrix :
    IFormattable,
    IEquatable<Matrix>,
    IEqualityOperators<Matrix, Matrix, bool>,
    IReadable<Matrix>
{
    public bool HasScale;

    public bool HasRotation;

    public Vector2 Scale;

    public Vector2 Rotation;

    public Point Translation;

    public bool IsEmpty =>
        Translation.IsZero && !HasScale && !HasRotation;

    public Matrix(bool hasScale, bool hasRotation, Vector2 scale, Vector2 rotation, Point translation)
    {
        HasScale = hasScale;
        HasRotation = hasRotation;
        Scale = scale;
        Rotation = rotation;
        Translation = translation;
    }

    public Vector2 Transform(Vector2 vector)
    {
        var x = vector.X * (HasScale ? Scale.X : 1) + vector.Y * (HasRotation ? Rotation.Y : 0) + Translation.X;
        var y = vector.X * (HasRotation ? Rotation.X : 0) + vector.Y * (HasScale ? Scale.Y : 1) + Translation.Y;

        return new Vector2(x, y);
    }

    public Point Transform(Point point)
    {
        var x = (int)(point.X * (HasScale ? Scale.X : 1) + point.Y * (HasRotation ? Rotation.Y : 0) + Translation.X);
        var y = (int)(point.X * (HasRotation ? Rotation.X : 0) + point.Y * (HasScale ? Scale.Y : 1) + Translation.Y);

        return new Point(x, y);
    }

    public Rectangle Transform(Rectangle rectangle)
    {
        var topLeft = Transform(rectangle.TopLeft);
        var bottomRight = Transform(rectangle.BottomRight);

        var xMin = Math.Min(topLeft.X, bottomRight.X);
        var yMin = Math.Min(topLeft.Y, bottomRight.Y);
        var xMax = Math.Max(topLeft.X, bottomRight.X);
        var yMax = Math.Max(topLeft.Y, bottomRight.Y);

        return new Rectangle(xMin, xMax, yMin, yMax);
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
        var bits = new BitReader();

        var hasScale = bits.ReadBit(ref reader);
        var scale = Vector2.Zero;

        if (hasScale)
        {
            var nScaleBits = bits.ReadIBits(ref reader, 5);
            scale.X = bits.ReadFBits(ref reader, nScaleBits);
            scale.Y = bits.ReadFBits(ref reader, nScaleBits);
        }

        var hasRotation = bits.ReadBit(ref reader);
        var rotation = Vector2.Zero;

        if (hasRotation)
        {
            var nRotationBits = bits.ReadIBits(ref reader, 5);
            rotation.X = bits.ReadFBits(ref reader, nRotationBits);
            rotation.Y = bits.ReadFBits(ref reader, nRotationBits);
        }

        var nTranslationBits = bits.ReadIBits(ref reader, 5);
        var translation = Point.Zero;

        translation.X = bits.ReadSBits(ref reader, nTranslationBits);
        translation.Y = bits.ReadSBits(ref reader, nTranslationBits);

        return new Matrix(hasScale, hasRotation, scale, rotation, translation);
    }
}
