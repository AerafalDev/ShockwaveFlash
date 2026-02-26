// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Filter;

public sealed record ConvolutionFilter(byte NumMatrixRows, byte NumMatrixColumns, float[] Matrix, float Divisor, float Bias, Color DefaultColor, ConvolutionFilterFlags Flags) : Filter
{
    public bool IsClamped =>
        Flags.HasFlag(ConvolutionFilterFlags.Clamp);

    public bool IsPreserveAlpha =>
        Flags.HasFlag(ConvolutionFilterFlags.PreserveAlpha);

    public static new ConvolutionFilter Decode(ref SpanReader reader)
    {
        var numMatrixRows = reader.ReadUInt8();
        var numMatrixColumns = reader.ReadUInt8();
        var divisor = reader.ReadFloat32();
        var bias = reader.ReadFloat32();

        var numMatrix = numMatrixRows * numMatrixColumns;
        var matrix = new float[numMatrix];

        for (var i = 0; i < numMatrix; i++)
            matrix[i] = reader.ReadFloat32();

        var defaultColor = Color.DecodeRgba(ref reader);
        var flags = (ConvolutionFilterFlags)reader.ReadUInt8();

        return new ConvolutionFilter(numMatrixRows, numMatrixColumns, matrix, divisor, bias, defaultColor, flags);
    }
}
