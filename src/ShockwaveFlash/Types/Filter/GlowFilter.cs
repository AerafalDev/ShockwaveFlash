namespace ShockwaveFlash.Types.Filter;

public sealed record GlowFilter(Color Color, FixedPoint2 Blur, Fixed8 Strength, GlowFilterFlags Flags) : Filter
{
    public bool IsInner =>
        Flags.HasFlag(GlowFilterFlags.InnerGlow);

    public bool IsKnockout =>
        Flags.HasFlag(GlowFilterFlags.Knockout);

    public bool IsCompositeSource =>
        Flags.HasFlag(GlowFilterFlags.CompositeSource);

    public byte Passes =>
        (byte)(Flags & GlowFilterFlags.Passes);

    public BlurFilter GetInnerBlurFilter()
    {
        return new BlurFilter(Blur, BlurFilterFlags.FromPasses(Passes));
    }

    public static GlowFilter DecodeBody(MemoryReader reader)
    {
        var color = Color.DecodeRgba(reader);
        var blur = reader.ReadFixedPoint2();
        var strength = reader.ReadFixed8();
        var flags = (GlowFilterFlags)reader.ReadUInt8();

        return new GlowFilter(color, blur, strength, flags);
    }

    public void EncodeBody(MemoryWriter writer)
    {
        Color.EncodeRgba(writer);
        writer.WriteFixedPoint2(Blur);
        writer.WriteFixed8(Strength);
        writer.WriteUInt8((byte)Flags);
    }
}
