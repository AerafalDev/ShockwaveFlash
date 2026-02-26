// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Types.Font;

public sealed class FontGlyph
{
    public IReadOnlyList<ShapeRecord> Shapes { get; set; }

    public ushort Code { get; set; }

    public ushort Advance { get; set; }

    public Rectangle? Bounds { get; set; }

    public FontGlyph(IReadOnlyList<ShapeRecord> shapes, ushort code, ushort advance, Rectangle? bounds)
    {
        Shapes = shapes;
        Code = code;
        Advance = advance;
        Bounds = bounds;
    }
}
