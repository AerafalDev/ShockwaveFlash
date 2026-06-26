namespace ShockwaveFlash.Types.Filter;

public sealed class GlowFilter : Filter
{
    public Color Color { get; set; }

    public FixedPoint2 Blur { get; set; }

    public Fixed8 Strength { get; set; }

    public GlowFilterFlags Flags { get; set; }

    public bool IsInner =>
        Flags.HasFlag(GlowFilterFlags.InnerGlow);

    public bool IsKnockout =>
        Flags.HasFlag(GlowFilterFlags.Knockout);

    public bool IsCompositeSource =>
        Flags.HasFlag(GlowFilterFlags.CompositeSource);

    public byte Passes =>
        (byte)(Flags & GlowFilterFlags.Passes);

    public GlowFilter(Color color, FixedPoint2 blur, Fixed8 strength, GlowFilterFlags flags)
    {
        Color = color;
        Blur = blur;
        Strength = strength;
        Flags = flags;
    }

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
