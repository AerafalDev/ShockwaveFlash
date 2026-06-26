namespace ShockwaveFlash.Types.Filter;

public sealed class DropShadowFilter : Filter
{
    public Color Color { get; set; }

    public FixedPoint2 Blur { get; set; }

    public Fixed16 Angle { get; set; }

    public Fixed16 Distance { get; set; }

    public Fixed8 Strength { get; set; }

    public DropShadowFilterFlags Flags { get; set; }

    public bool IsInner =>
        Flags.HasFlag(DropShadowFilterFlags.InnerShadow);

    public bool IsKnockout =>
        Flags.HasFlag(DropShadowFilterFlags.Knockout);

    public bool HideObject =>
        !Flags.HasFlag(DropShadowFilterFlags.CompositeSource);

    public byte Passes =>
        (byte)(Flags & DropShadowFilterFlags.Passes);

    public DropShadowFilter(Color color, FixedPoint2 blur, Fixed16 angle, Fixed16 distance, Fixed8 strength, DropShadowFilterFlags flags)
    {
        Color = color;
        Blur = blur;
        Angle = angle;
        Distance = distance;
        Strength = strength;
        Flags = flags;
    }

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
