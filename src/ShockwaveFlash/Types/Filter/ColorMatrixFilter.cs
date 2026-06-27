namespace ShockwaveFlash.Types.Filter;

public sealed class ColorMatrixFilter : Filter
{
    public float[] Matrix { get; set; }

    public static ColorMatrixFilter Identity =>
        new([
            1f, 0f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f, 0f,
            0f, 0f, 1f, 0f, 0f,
            0f, 0f, 0f, 1f, 0f,
        ]);

    public bool Impotent =>
        Matrix.SequenceEqual(Identity.Matrix);

    public ColorMatrixFilter(float[] matrix)
    {
        Matrix = matrix;
    }

    public ColorMatrixFilter() : this(Identity.Matrix)
    {
    }

    public static ColorMatrixFilter DecodeBody(MemoryReader reader)
    {
        var matrix = new float[20];

        for (var i = 0; i < matrix.Length; i++)
            matrix[i] = reader.ReadFloat32();

        return new ColorMatrixFilter(matrix);
    }

    public void EncodeBody(MemoryWriter writer)
    {
        for (var i = 0; i < Matrix.Length; i++)
            writer.WriteFloat32(Matrix[i]);
    }
}
