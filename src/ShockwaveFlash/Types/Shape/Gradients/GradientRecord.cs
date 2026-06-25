namespace ShockwaveFlash.Types.Shape.Gradients;

public sealed record GradientRecord(byte Ratio, Color Color)
{
    public static GradientRecord Decode(MemoryReader reader, byte shapeVersion)
    {
        return new GradientRecord(reader.ReadUInt8(), shapeVersion >= 3 ? Color.DecodeRgba(reader) : Color.DecodeRgb(reader));
    }

    public void Encode(MemoryWriter writer, byte shapeVersion)
    {
        writer.WriteUInt8(Ratio);

        if (shapeVersion >= 3)
            Color.EncodeRgba(writer);
        else
            Color.EncodeRgb(writer);
    }
}
