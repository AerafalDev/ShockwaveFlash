namespace ShockwaveFlash.Types.Shape.Gradients;

public sealed class Gradient
{
    public Matrix Matrix { get; set; }

    public GradientSpread Spread { get; set; }

    public GradientInterpolation Interpolation { get; set; }

    public GradientRecord[] Records { get; set; }

    public Gradient(Matrix matrix, GradientSpread spread, GradientInterpolation interpolation, GradientRecord[] records)
    {
        Matrix = matrix;
        Spread = spread;
        Interpolation = interpolation;
        Records = records;
    }

    public static Gradient Decode(MemoryReader reader, byte shapeVersion)
    {
        var matrix = Matrix.Decode(reader);

        var (numRecords, spread, interpolation) = DecodeFlags(reader);

        if (numRecords is 0)
            return new Gradient(matrix, spread, interpolation, []);

        var records = new GradientRecord[numRecords];

        for (var i = 0; i < numRecords; i++)
            records[i] = GradientRecord.Decode(reader, shapeVersion);

        return new Gradient(matrix, spread, interpolation, records);
    }

    public static (Gradient, Gradient) DecodeMorph(MemoryReader reader)
    {
        var startMatrix = Matrix.Decode(reader);
        var endMatrix = Matrix.Decode(reader);

        var (numRecords, spread, interpolation) = DecodeFlags(reader);

        var startRecords = new GradientRecord[numRecords];
        var endRecords = new GradientRecord[numRecords];

        for (var i = 0; i < numRecords; i++)
        {
            startRecords[i] = new GradientRecord(reader.ReadUInt8(), Color.DecodeRgba(reader));
            endRecords[i] = new GradientRecord(reader.ReadUInt8(), Color.DecodeRgba(reader));
        }

        return (new Gradient(startMatrix, spread, interpolation, startRecords), new Gradient(endMatrix, spread, interpolation, endRecords));
    }

    public void Encode(MemoryWriter writer, byte shapeVersion)
    {
        Matrix.Encode(writer);

        EncodeFlags(writer, Records.Length);

        foreach (var record in Records)
            record.Encode(writer, shapeVersion);
    }

    public void EncodeMorph(MemoryWriter writer, Gradient end)
    {
        Matrix.Encode(writer);
        end.Matrix.Encode(writer);

        EncodeFlags(writer, Records.Length);

        for (var i = 0; i < Records.Length; i++)
        {
            writer.WriteUInt8(Records[i].Ratio);
            Records[i].Color.EncodeRgba(writer);
            writer.WriteUInt8(end.Records[i].Ratio);
            end.Records[i].Color.EncodeRgba(writer);
        }
    }

    private static (int, GradientSpread, GradientInterpolation) DecodeFlags(MemoryReader reader)
    {
        var flags = reader.ReadUInt8();
        var spread = GradientSpread.Parse((byte)((flags >> 6) & 3));
        var interpolation = GradientInterpolation.Parse((byte)((flags >> 4) & 3));
        var numRecords = flags & 15;
        return (numRecords, spread, interpolation);
    }

    private void EncodeFlags(MemoryWriter writer, int numRecords)
    {
        var flags = (byte)((((byte)Spread & 3) << 6) | (((byte)Interpolation & 3) << 4) | (numRecords & 15));
        writer.WriteUInt8(flags);
    }
}
