namespace ShockwaveFlash.Types;

public readonly record struct Color(byte R, byte G, byte B, byte A)
{
    public static Color Transparent => new(0, 0, 0, 0);

    public static Color Black => new(0, 0, 0, 255);

    public static Color White => new(255, 255, 255, 255);

    public Color(float r, float g, float b, float a)
        : this((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255))
    {
    }

    public Color(int color)
        : this((byte)((color >> 16) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)(color & 0xFF), (byte)((color >> 24) & 0xFF))
    {
    }

    public (float R, float G, float B, float A) ToFloat()
    {
        return (R / 255f, G / 255f, B / 255f, A / 255f);
    }

    public int ToInt()
    {
        return A << 24 | R << 16 | G << 8 | B;
    }

    public string ToHexArgb()
    {
        return $"#{A:X2}{R:X2}{G:X2}{B:X2}";
    }

    public string ToHexRgba()
    {
        return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }

    public string ToHexRgb()
    {
        return $"#{R:X2}{G:X2}{B:X2}";
    }

    internal static Color DecodeArgb(MemoryReader reader)
    {
        var a = reader.ReadUInt8();
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();

        return new Color(r, g, b, a);
    }

    internal static Color DecodeRgba(MemoryReader reader)
    {
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();
        var a = reader.ReadUInt8();

        return new Color(r, g, b, a);
    }

    internal static Color DecodeRgb(MemoryReader reader)
    {
        var r = reader.ReadUInt8();
        var g = reader.ReadUInt8();
        var b = reader.ReadUInt8();

        return new Color(r, g, b, 255);
    }

    internal void EncodeArgb(MemoryWriter writer)
    {
        writer.WriteUInt8(A);
        writer.WriteUInt8(R);
        writer.WriteUInt8(G);
        writer.WriteUInt8(B);
    }

    internal void EncodeRgba(MemoryWriter writer)
    {
        writer.WriteUInt8(R);
        writer.WriteUInt8(G);
        writer.WriteUInt8(B);
        writer.WriteUInt8(A);
    }

    internal void EncodeRgb(MemoryWriter writer)
    {
        writer.WriteUInt8(R);
        writer.WriteUInt8(G);
        writer.WriteUInt8(B);
    }
}
