namespace ShockwaveFlash.Types;

public readonly record struct Rectangle(int XMin, int XMax, int YMin, int YMax)
{
    public int Width => Math.Max(XMax - XMin, 0);

    public int Height => Math.Max(YMax - YMin, 0);

    public Point TopLeft => new(XMin, YMin);

    public Point BottomRight => new(XMax, YMax);

    public Point TopRight => new(XMax, YMin);

    public Point BottomLeft => new(XMin, YMax);

    public Point Center => new(XMin + Width / 2, YMin + Height / 2);

    internal static Rectangle Decode(MemoryReader reader)
    {
        var bits = new BitReader();

        var nBits = bits.ReadIBits(reader, 5);

        var xMin = bits.ReadSBits(reader, nBits);
        var xMax = bits.ReadSBits(reader, nBits);
        var yMin = bits.ReadSBits(reader, nBits);
        var yMax = bits.ReadSBits(reader, nBits);

        return new Rectangle(xMin, xMax, yMin, yMax);
    }

    internal void Encode(MemoryWriter writer)
    {
        var bits = new BitWriter();

        var nBits = Math.Max(
            Math.Max(BitWriter.SignedBitsNeeded(XMin), BitWriter.SignedBitsNeeded(XMax)),
            Math.Max(BitWriter.SignedBitsNeeded(YMin), BitWriter.SignedBitsNeeded(YMax)));

        bits.WriteUBits(writer, (uint)nBits, 5);
        bits.WriteSBits(writer, XMin, nBits);
        bits.WriteSBits(writer, XMax, nBits);
        bits.WriteSBits(writer, YMin, nBits);
        bits.WriteSBits(writer, YMax, nBits);
        bits.Flush(writer);
    }
}
