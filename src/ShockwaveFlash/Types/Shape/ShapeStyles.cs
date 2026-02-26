// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

public sealed record ShapeStyles(FillStyle[] FillStyles, LineStyle[] LineStyles)
{
    public static (ShapeStyles, byte, byte) Decode(ref SpanReader reader, byte swfVersion, byte shapeVersion)
    {
        ushort numFillStyle = reader.ReadUInt8();

        if (numFillStyle is 255 && shapeVersion >= 2)
            numFillStyle = reader.ReadUInt16();

        var fillStyles = new FillStyle[numFillStyle];

        for (var i = 0; i < numFillStyle; i++)
            fillStyles[i] = FillStyle.Decode(ref reader, swfVersion, shapeVersion);

        ushort numLineStyle = reader.ReadUInt8();

        if (numLineStyle is 255 && shapeVersion >= 2)
            numLineStyle = reader.ReadUInt16();

        var lineStyles = new LineStyle[numLineStyle];

        for (var i = 0; i < numLineStyle; i++)
            lineStyles[i] = LineStyle.Decode(ref reader, swfVersion, shapeVersion);

        var numBits = reader.ReadUInt8();

        return (new ShapeStyles(fillStyles, lineStyles), (byte)(numBits >> 4), (byte)(numBits & 15));
    }
}
