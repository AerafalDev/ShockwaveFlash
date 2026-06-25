namespace ShockwaveFlash.Types.Filter;

public sealed record BevelFilter(Color ShadowColor, Color HighlightColor, FixedPoint2 Blur, Fixed16 Angle, Fixed16 Distance, Fixed8 Strength, BevelFilterFlags Flags) : Filter
{
    public bool IsInner =>
        Flags.HasFlag(BevelFilterFlags.InnerShadow);

    public bool IsKnockout =>
        Flags.HasFlag(BevelFilterFlags.Knockout);

    public bool IsOnTop =>
        Flags.HasFlag(BevelFilterFlags.OnTop);

    public byte Passes =>
        (byte)(Flags & BevelFilterFlags.Passes);

    public BlurFilter GetInnerBlurFilter()
    {
        return new BlurFilter(Blur, BlurFilterFlags.FromPasses(Passes));
    }

    public static BevelFilter DecodeBody(MemoryReader reader)
    {
        var highlightColor = Color.DecodeRgba(reader);
        var shadowColor = Color.DecodeRgba(reader);
        var blur = reader.ReadFixedPoint2();
        var angle = reader.ReadFixed();
        var distance = reader.ReadFixed();
        var strength = reader.ReadFixed8();
        var flags = (BevelFilterFlags)reader.ReadUInt8();

        return new BevelFilter(shadowColor, highlightColor, blur, angle, distance, strength, flags);
    }

    public void EncodeBody(MemoryWriter writer)
    {
        HighlightColor.EncodeRgba(writer);
        ShadowColor.EncodeRgba(writer);
        writer.WriteFixedPoint2(Blur);
        writer.WriteFixed(Angle);
        writer.WriteFixed(Distance);
        writer.WriteFixed8(Strength);
        writer.WriteUInt8((byte)Flags);
    }
}
