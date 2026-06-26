using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Filter;

public sealed class GradientFilter : Filter
{
    public GradientRecord[] Colors { get; set; }

    public FixedPoint2 Blur { get; set; }

    public Fixed16 Angle { get; set; }

    public Fixed16 Distance { get; set; }

    public Fixed8 Strength { get; set; }

    public GradientFilterFlags Flags { get; set; }

    public FilterType Type { get; set; }

    public bool IsInner =>
        Flags.HasFlag(GradientFilterFlags.InnerShadow);

    public bool IsKnockout =>
        Flags.HasFlag(GradientFilterFlags.Knockout);

    public bool IsOnTop =>
        Flags.HasFlag(GradientFilterFlags.OnTop);

    public bool IsCompositeSource =>
        Flags.HasFlag(GradientFilterFlags.CompositeSource);

    public byte Passes =>
        (byte)(Flags & GradientFilterFlags.Passes);

    public GradientFilter(GradientRecord[] colors, FixedPoint2 blur, Fixed16 angle, Fixed16 distance, Fixed8 strength, GradientFilterFlags flags, FilterType type)
    {
        Colors = colors;
        Blur = blur;
        Angle = angle;
        Distance = distance;
        Strength = strength;
        Flags = flags;
        Type = type;
    }

    public BlurFilter GetInnerBlurFilter()
    {
        return new BlurFilter(Blur, BlurFilterFlags.FromPasses(Passes));
    }

    public static GradientFilter DecodeBody(MemoryReader reader, FilterType type)
    {
        var numColors = reader.ReadUInt8();
        var colors = new Color[numColors];

        for (var i = 0; i < numColors; i++)
            colors[i] = Color.DecodeRgba(reader);

        var records = new GradientRecord[numColors];

        for (var i = 0; i < numColors; i++)
            records[i] = new GradientRecord(reader.ReadUInt8(), colors[i]);

        var blur = reader.ReadFixedPoint2();
        var angle = reader.ReadFixed();
        var distance = reader.ReadFixed();
        var strength = reader.ReadFixed8();
        var flags = (GradientFilterFlags)reader.ReadUInt8();

        return new GradientFilter(records, blur, angle, distance, strength, flags, type);
    }

    public void EncodeBody(MemoryWriter writer)
    {
        writer.WriteUInt8((byte)Colors.Length);

        for (var i = 0; i < Colors.Length; i++)
            Colors[i].Color.EncodeRgba(writer);

        for (var i = 0; i < Colors.Length; i++)
            writer.WriteUInt8(Colors[i].Ratio);

        writer.WriteFixedPoint2(Blur);
        writer.WriteFixed(Angle);
        writer.WriteFixed(Distance);
        writer.WriteFixed8(Strength);
        writer.WriteUInt8((byte)Flags);
    }
}
