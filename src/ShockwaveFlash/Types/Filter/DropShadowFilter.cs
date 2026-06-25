namespace ShockwaveFlash.Types.Filter;

public sealed record DropShadowFilter(Color Color, FixedPoint2 Blur, Fixed16 Angle, Fixed16 Distance, Fixed8 Strength, DropShadowFilterFlags Flags) : Filter
{
    public bool IsInner =>
        Flags.HasFlag(DropShadowFilterFlags.InnerShadow);

    public bool IsKnockout =>
        Flags.HasFlag(DropShadowFilterFlags.Knockout);

    public bool HideObject =>
        !Flags.HasFlag(DropShadowFilterFlags.CompositeSource);

    public byte Passes =>
        (byte)(Flags & DropShadowFilterFlags.Passes);

    public BlurFilter GetInnerBlurFilter()
    {
        return new BlurFilter(Blur, BlurFilterFlags.FromPasses(Passes));
    }

    public GlowFilter GetInnerGlowFilter()
    {
        var flags = GlowFilterFlags.FromPasses(Passes)
            .Set(GlowFilterFlags.InnerGlow, IsInner)
            .Set(GlowFilterFlags.Knockout, IsKnockout)
            .Set(GlowFilterFlags.CompositeSource, !HideObject);

        return new GlowFilter(Color, Blur, Strength, flags);
    }

    public static DropShadowFilter DecodeBody(MemoryReader reader)
    {
        var color = Color.DecodeRgba(reader);
        var blur = reader.ReadFixedPoint2();
        var angle = reader.ReadFixed();
        var distance = reader.ReadFixed();
        var strength = reader.ReadFixed8();
        var flags = (DropShadowFilterFlags)reader.ReadUInt8();

        return new DropShadowFilter(color, blur, angle, distance, strength, flags);
    }

    public void EncodeBody(MemoryWriter writer)
    {
        Color.EncodeRgba(writer);
        writer.WriteFixedPoint2(Blur);
        writer.WriteFixed(Angle);
        writer.WriteFixed(Distance);
        writer.WriteFixed8(Strength);
        writer.WriteUInt8((byte)Flags);
    }
}
