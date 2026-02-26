// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Filter;

public sealed record ColorMatrixFilter(float[] Matrix) : Filter
{
    public static ColorMatrixFilter Identity =>
        new([
            1f, 0f, 0f, 0f, 0f, // r
            0f, 1f, 0f, 0f, 0f, // g
            0f, 0f, 1f, 0f, 0f, // b
            0f, 0f, 0f, 1f, 0f, // a
        ]);

    public bool Impotent =>
        Matrix.SequenceEqual(Identity.Matrix);

    public ColorMatrixFilter() : this(Identity.Matrix)
    {
    }

    public static new ColorMatrixFilter Decode(ref SpanReader reader)
    {
        var matrix = new float[20];

        for (var i = 0; i < matrix.Length; i++)
            matrix[i] = reader.ReadFloat32();

        return new ColorMatrixFilter(matrix);
    }
}
