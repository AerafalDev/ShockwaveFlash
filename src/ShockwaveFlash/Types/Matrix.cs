namespace ShockwaveFlash.Types;

public readonly record struct Matrix(FixedPoint2 Scale, FixedPoint2 Rotation, Point Translation)
{
    public static Matrix Identity => new(new FixedPoint2(Fixed16.One, Fixed16.One), FixedPoint2.Zero, Point.Zero);

    public static Matrix Decode(MemoryReader reader)
    {
        var bits = new BitReader();

        var scale = new FixedPoint2(Fixed16.One, Fixed16.One);

        if (bits.ReadBit(reader))
        {
            var nScaleBits = bits.ReadIBits(reader, 5);
            scale = new FixedPoint2(bits.ReadFBits(reader, nScaleBits), bits.ReadFBits(reader, nScaleBits));
        }

        var rotation = FixedPoint2.Zero;

        if (bits.ReadBit(reader))
        {
            var nRotationBits = bits.ReadIBits(reader, 5);
            rotation = new FixedPoint2(bits.ReadFBits(reader, nRotationBits), bits.ReadFBits(reader, nRotationBits));
        }

        var nTranslationBits = bits.ReadIBits(reader, 5);
        var translation = new Point(bits.ReadSBits(reader, nTranslationBits), bits.ReadSBits(reader, nTranslationBits));

        return new Matrix(scale, rotation, translation);
    }

    public void Encode(MemoryWriter writer)
    {
        var bits = new BitWriter();

        var hasScale = Scale.X != Fixed16.One || Scale.Y != Fixed16.One;

        bits.WriteBit(writer, hasScale);

        if (hasScale)
        {
            var nScaleBits = Math.Max(BitWriter.SignedBitsNeeded(Scale.X.Raw), BitWriter.SignedBitsNeeded(Scale.Y.Raw));
            bits.WriteUBits(writer, (uint)nScaleBits, 5);
            bits.WriteFBits(writer, Scale.X, nScaleBits);
            bits.WriteFBits(writer, Scale.Y, nScaleBits);
        }

        var hasRotation = Rotation.X != Fixed16.Zero || Rotation.Y != Fixed16.Zero;

        bits.WriteBit(writer, hasRotation);

        if (hasRotation)
        {
            var nRotationBits = Math.Max(BitWriter.SignedBitsNeeded(Rotation.X.Raw), BitWriter.SignedBitsNeeded(Rotation.Y.Raw));
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
}
