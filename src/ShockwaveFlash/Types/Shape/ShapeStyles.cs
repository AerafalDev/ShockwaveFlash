// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

public sealed record ShapeStyles(FillStyle[] FillStyles, LineStyle[] LineStyles)
{
    public static (ShapeStyles, byte, byte) Decode(MemoryReader reader, byte swfVersion, byte shapeVersion)
    {
        ushort numFillStyle = reader.ReadUInt8();

        if (numFillStyle is 255 && shapeVersion >= 2)
            numFillStyle = reader.ReadUInt16();

        var fillStyles = new FillStyle[numFillStyle];

        for (var i = 0; i < numFillStyle; i++)
            fillStyles[i] = FillStyle.Decode(reader, swfVersion, shapeVersion);

        ushort numLineStyle = reader.ReadUInt8();

        if (numLineStyle is 255 && shapeVersion >= 2)
            numLineStyle = reader.ReadUInt16();

        var lineStyles = new LineStyle[numLineStyle];

        for (var i = 0; i < numLineStyle; i++)
            lineStyles[i] = LineStyle.Decode(reader, swfVersion, shapeVersion);

        var numBits = reader.ReadUInt8();

        return (new ShapeStyles(fillStyles, lineStyles), (byte)(numBits >> 4), (byte)(numBits & 15));
    }

    public (byte, byte) Encode(MemoryWriter writer, byte swfVersion, byte shapeVersion)
    {
        if (FillStyles.Length >= 255 && shapeVersion >= 2)
        {
            writer.WriteUInt8(255);
            writer.WriteUInt16((ushort)FillStyles.Length);
        }
        else
        {
            writer.WriteUInt8((byte)FillStyles.Length);
        }

        foreach (var fillStyle in FillStyles)
            fillStyle.Encode(writer, swfVersion, shapeVersion);

        if (LineStyles.Length >= 255 && shapeVersion >= 2)
        {
            writer.WriteUInt8(255);
            writer.WriteUInt16((ushort)LineStyles.Length);
        }
        else
        {
            writer.WriteUInt8((byte)LineStyles.Length);
        }

        foreach (var lineStyle in LineStyles)
            lineStyle.Encode(writer, swfVersion, shapeVersion);

        var numFillBits = (byte)BitWriter.UnsignedBitsNeeded((uint)FillStyles.Length);
        var numLineBits = (byte)BitWriter.UnsignedBitsNeeded((uint)LineStyles.Length);

        writer.WriteUInt8((byte)((numFillBits << 4) | (numLineBits & 15)));

        return (numFillBits, numLineBits);
    }
}
