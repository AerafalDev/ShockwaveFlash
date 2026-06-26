namespace ShockwaveFlash.Types.Filter;

public sealed class BlurFilter : Filter
{
    public FixedPoint2 Blur { get; set; }

    public BlurFilterFlags Flags { get; set; }

    public byte Passes =>
        (byte)((byte)(Flags & BlurFilterFlags.Passes) >> 3);

    public bool Impotent =>
        Passes is 0 || (Blur.X.Raw <= 1 << 16 && Blur.Y.Raw <= 1 << 16);

    public BlurFilter(FixedPoint2 blur, BlurFilterFlags flags)
    {
        Blur = blur;
        Flags = flags;
    }

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
