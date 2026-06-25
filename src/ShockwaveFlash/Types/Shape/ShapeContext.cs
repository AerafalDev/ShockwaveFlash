// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

/// <summary>
/// Mutable shape-parsing context threaded through <see cref="ShapeRecord"/> decoding.
/// Reference type so style-change updates to the fill/line bit counts propagate without <c>ref</c>.
/// </summary>
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
