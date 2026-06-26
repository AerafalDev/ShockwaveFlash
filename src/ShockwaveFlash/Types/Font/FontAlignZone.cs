namespace ShockwaveFlash.Types.Font;

public sealed class FontAlignZone
{
    public float Left { get; set; }

    public float Width { get; set; }

    public float Bottom { get; set; }

    public float Height { get; set; }

    public FontAlignZone(float left, float width, float bottom, float height)
    {
        Left = left;
        Width = width;
        Bottom = bottom;
        Height = height;
    }

    public static FontAlignZone Decode(MemoryReader reader)
    {
        reader.Advance(sizeof(byte));
        var zone = new FontAlignZone(reader.ReadFloat16(), reader.ReadFloat16(), reader.ReadFloat16(), reader.ReadFloat16());
        reader.Advance(sizeof(byte));
        return zone;
    }

    public void Encode(MemoryWriter writer)
    {
        writer.WriteUInt8(2);
        writer.WriteFloat16(Left);
        writer.WriteFloat16(Width);
        writer.WriteFloat16(Bottom);
        writer.WriteFloat16(Height);
        writer.WriteUInt8(0);
    }
}
