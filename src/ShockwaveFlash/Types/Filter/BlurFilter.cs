namespace ShockwaveFlash.Types.Filter;

public sealed record BlurFilter(FixedPoint2 Blur, BlurFilterFlags Flags) : Filter
{
    public byte Passes =>
        (byte)((byte)(Flags & BlurFilterFlags.Passes) >> 3);

    public bool Impotent =>
        Passes is 0 || (Blur.X.Raw <= 1 << 16 && Blur.Y.Raw <= 1 << 16);

    public static BlurFilter DecodeBody(MemoryReader reader)
    {
        var blur = reader.ReadFixedPoint2();
        var flags = (BlurFilterFlags)reader.ReadUInt8();

        return new BlurFilter(blur, flags);
    }

    public void EncodeBody(MemoryWriter writer)
    {
        writer.WriteFixedPoint2(Blur);
        writer.WriteUInt8((byte)Flags);
    }
}
