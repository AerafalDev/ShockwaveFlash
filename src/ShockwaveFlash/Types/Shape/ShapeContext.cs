namespace ShockwaveFlash.Types.Shape;

public sealed class ShapeContext
{
    public byte SwfVersion { get; }

    public byte ShapeVersion { get; }

    public int NumFillBits { get; set; }

    public int NumLineBits { get; set; }

    public ShapeContext(byte swfVersion, byte shapeVersion, int numFillBits, int numLineBits)
    {
        SwfVersion = swfVersion;
        ShapeVersion = shapeVersion;
        NumFillBits = numFillBits;
        NumLineBits = numLineBits;
    }
}
