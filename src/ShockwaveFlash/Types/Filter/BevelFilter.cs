namespace ShockwaveFlash.Types.Filter;

public sealed class BevelFilter : Filter
{
    public Color ShadowColor { get; set; }

    public Color HighlightColor { get; set; }

    public FixedPoint2 Blur { get; set; }

    public Fixed16 Angle { get; set; }

    public Fixed16 Distance { get; set; }

    public Fixed8 Strength { get; set; }

    public BevelFilterFlags Flags { get; set; }

    public bool IsInner =>
        Flags.HasFlag(BevelFilterFlags.InnerShadow);

    public bool IsKnockout =>
        Flags.HasFlag(BevelFilterFlags.Knockout);

    public bool IsOnTop =>
        Flags.HasFlag(BevelFilterFlags.OnTop);

    public byte Passes =>
        (byte)(Flags & BevelFilterFlags.Passes);

    public BevelFilter(Color shadowColor, Color highlightColor, FixedPoint2 blur, Fixed16 angle, Fixed16 distance, Fixed8 strength, BevelFilterFlags flags)
    {
        ShadowColor = shadowColor;
        HighlightColor = highlightColor;
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
