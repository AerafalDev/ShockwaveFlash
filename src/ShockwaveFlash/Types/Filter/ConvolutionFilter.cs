namespace ShockwaveFlash.Types.Filter;

public sealed record ConvolutionFilter(byte NumMatrixRows, byte NumMatrixColumns, float[] Matrix, float Divisor, float Bias, Color DefaultColor, ConvolutionFilterFlags Flags) : Filter
{
    public bool IsClamped =>
        Flags.HasFlag(ConvolutionFilterFlags.Clamp);

    public bool IsPreserveAlpha =>
        Flags.HasFlag(ConvolutionFilterFlags.PreserveAlpha);

    public static ConvolutionFilter DecodeBody(MemoryReader reader)
    {
        var numMatrixRows = reader.ReadUInt8();
        var numMatrixColumns = reader.ReadUInt8();
        var divisor = reader.ReadFloat32();
        var bias = reader.ReadFloat32();

        var numMatrix = numMatrixRows * numMatrixColumns;
        var matrix = new float[numMatrix];

        for (var i = 0; i < numMatrix; i++)
            matrix[i] = reader.ReadFloat32();

        var defaultColor = Color.DecodeRgba(reader);
        var flags = (ConvolutionFilterFlags)reader.ReadUInt8();

        return new ConvolutionFilter(numMatrixRows, numMatrixColumns, matrix, divisor, bias, defaultColor, flags);
    }

    public void EncodeBody(MemoryWriter writer)
    {
        writer.WriteUInt8(NumMatrixRows);
        writer.WriteUInt8(NumMatrixColumns);
        writer.WriteFloat32(Divisor);
        writer.WriteFloat32(Bias);

        var numMatrix = NumMatrixRows * NumMatrixColumns;

        for (var i = 0; i < numMatrix; i++)
            writer.WriteFloat32(Matrix[i]);

        DefaultColor.EncodeRgba(writer);
        writer.WriteUInt8((byte)Flags);
    }
}
