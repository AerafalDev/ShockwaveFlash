// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape.Gradients;

public sealed record Gradient(Matrix Matrix, GradientSpread Spread, GradientInterpolation Interpolation, GradientRecord[] Records)
{
    public static Gradient Decode(ref SpanReader reader, byte shapeVersion)
    {
        var matrix = Matrix.Decode(ref reader);

        var (numRecords, spread, interpolation) = DecodeFlags(ref reader);

        if (numRecords is 0)
            return new Gradient(matrix, spread, interpolation, []);

        var records = new GradientRecord[numRecords];

        for (var i = 0; i < numRecords; i++)
            records[i] = GradientRecord.Decode(ref reader, shapeVersion);

        return new Gradient(matrix, spread, interpolation, records);
    }

    public static (Gradient, Gradient) DecodeMorph(ref SpanReader reader)
    {
        var startMatrix = Matrix.Decode(ref reader);
        var endMatrix = Matrix.Decode(ref reader);

        var (numRecords, spread, interpolation) = DecodeFlags(ref reader);

        var startRecords = new GradientRecord[numRecords];
        var endRecords = new GradientRecord[numRecords];

        for (var i = 0; i < numRecords; i++)
        {
            startRecords[i] = new GradientRecord(reader.ReadUInt8(), Color.DecodeRgba(ref reader));
            endRecords[i] = new GradientRecord(reader.ReadUInt8(), Color.DecodeRgba(ref reader));
        }

        return (new Gradient(startMatrix, spread, interpolation, startRecords), new Gradient(endMatrix, spread, interpolation, endRecords));
    }

    private static (int, GradientSpread, GradientInterpolation) DecodeFlags(ref SpanReader reader)
    {
        var flags = reader.ReadUInt8();
        var spread = GradientSpread.Parse((byte)((flags >> 6) & 3));
        var interpolation = GradientInterpolation.Parse((byte)((flags >> 4) & 3));
        var numRecords = flags & 15;
        return (numRecords, spread, interpolation);
    }
}
