using System.Numerics;

namespace ShockwaveFlash.Types.Filter;

public sealed record BlurFilter(Vector2 Blur, BlurFilterFlags Flags) : Filter
{
    public byte Passes =>
        (byte)((byte)(Flags & BlurFilterFlags.Passes) >> 3);

    public bool Impotent =>
        Passes is 0 || Blur is { X: <= 1 << 16, Y: <= 1 << 16 };

    public static BlurFilter DecodeBody(MemoryReader reader)
    {
        var blur = reader.ReadVector2();
        var flags = (BlurFilterFlags)reader.ReadUInt8();

        return new BlurFilter(blur, flags);
    }

    public void EncodeBody(MemoryWriter writer)
    {
        writer.WriteVector2(Blur);
        writer.WriteUInt8((byte)Flags);
    }
}
