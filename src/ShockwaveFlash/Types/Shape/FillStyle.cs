// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Shape;

public abstract record FillStyle
{
    public static FillStyle Decode(MemoryReader reader, byte swfVersion, byte shapeVersion)
    {
        var fillStyleType = reader.ReadUInt8();

        switch ((FillStyleType)fillStyleType)
        {
            case FillStyleType.Solid:
                return new FillStyleSolid(shapeVersion >= 3 ? Color.DecodeRgba(reader) : Color.DecodeRgb(reader));

            case FillStyleType.LinearGradient:
                return new FillStyleLinearGradient(Gradient.Decode(reader, shapeVersion));

            case FillStyleType.RadialGradient:
                return new FillStyleRadialGradient(Gradient.Decode(reader, shapeVersion));

            case FillStyleType.FocalGradient:
                return new FillStyleFocalGradient(Gradient.Decode(reader, shapeVersion), reader.ReadFixed8());

            case FillStyleType.RepeatingBitmap:
            case FillStyleType.ClippedBitmap:
            case FillStyleType.NonSmoothedRepeatingBitmap:
            case FillStyleType.NonSmoothedClippedBitmap:
                return new FillStyleBitmap(reader.ReadUInt16(), Matrix.Decode(reader), swfVersion >= 8 && (fillStyleType & 2) is 0, (fillStyleType & 1) is 0);

            default:
                throw new NotSupportedException($"FillStyle {fillStyleType} is not supported.");
        }
    }

    public void Encode(MemoryWriter writer, byte swfVersion, byte shapeVersion)
    {
        switch (this)
        {
            case FillStyleSolid solid:
                writer.WriteUInt8((byte)FillStyleType.Solid);
                if (shapeVersion >= 3)
                    solid.Color.EncodeRgba(writer);
                else
                    solid.Color.EncodeRgb(writer);
                break;

            case FillStyleLinearGradient linear:
                writer.WriteUInt8((byte)FillStyleType.LinearGradient);
                linear.Gradient.Encode(writer, shapeVersion);
                break;

            case FillStyleRadialGradient radial:
                writer.WriteUInt8((byte)FillStyleType.RadialGradient);
                radial.Gradient.Encode(writer, shapeVersion);
                break;

            case FillStyleFocalGradient focal:
                writer.WriteUInt8((byte)FillStyleType.FocalGradient);
                focal.Gradient.Encode(writer, shapeVersion);
                writer.WriteFixed8(focal.FocalPoint);
                break;

            case FillStyleBitmap bitmap:
                writer.WriteUInt8((byte)(0x40 | (bitmap.IsSmoothed ? 0 : 2) | (bitmap.IsRepeating ? 0 : 1)));
                writer.WriteUInt16(bitmap.Id);
                bitmap.Matrix.Encode(writer);
                break;

            default:
                throw new NotSupportedException($"FillStyle {this} is not supported.");
        }
    }

    public static (FillStyle, FillStyle) DecodeMorph(MemoryReader reader)
    {
        var fillStyleType = reader.ReadUInt8();

        switch ((FillStyleType)fillStyleType)
        {
            case FillStyleType.Solid:
                return (new FillStyleSolid(Color.DecodeRgba(reader)), new FillStyleSolid(Color.DecodeRgba(reader)));

            case FillStyleType.LinearGradient:
                var (startGradient, endGradient) = Gradient.DecodeMorph(reader);
                return (new FillStyleLinearGradient(startGradient), new FillStyleLinearGradient(endGradient));

            case FillStyleType.RadialGradient:
                var (startGradient2, endGradient2) = Gradient.DecodeMorph(reader);
                return (new FillStyleRadialGradient(startGradient2), new FillStyleRadialGradient(endGradient2));

            case FillStyleType.FocalGradient:
                var (startGradient3, endGradient3) = Gradient.DecodeMorph(reader);
                var startFocalPoint = reader.ReadFixed8();
                var endFocalPoint = reader.ReadFixed8();
                return (new FillStyleFocalGradient(startGradient3, startFocalPoint), new FillStyleFocalGradient(endGradient3, endFocalPoint));

            case FillStyleType.RepeatingBitmap:
            case FillStyleType.ClippedBitmap:
            case FillStyleType.NonSmoothedRepeatingBitmap:
            case FillStyleType.NonSmoothedClippedBitmap:
                var id = reader.ReadUInt16();
                var startMatrix = Matrix.Decode(reader);
                var endMatrix = Matrix.Decode(reader);
                var isSmoothed = (fillStyleType & 2) is 0;
                var isRepeating = (fillStyleType & 1) is 0;
                return (new FillStyleBitmap(id, startMatrix, isSmoothed, isRepeating), new FillStyleBitmap(id, endMatrix, isSmoothed, isRepeating));

            default:
                throw new NotSupportedException($"FillStyle {fillStyleType} is not supported.");
        }
    }

    public static void EncodeMorph(MemoryWriter writer, FillStyle start, FillStyle end)
    {
        switch (start)
        {
            case FillStyleSolid startSolid when end is FillStyleSolid endSolid:
                writer.WriteUInt8((byte)FillStyleType.Solid);
                startSolid.Color.EncodeRgba(writer);
                endSolid.Color.EncodeRgba(writer);
                break;

            case FillStyleLinearGradient startLinear when end is FillStyleLinearGradient endLinear:
                writer.WriteUInt8((byte)FillStyleType.LinearGradient);
                startLinear.Gradient.EncodeMorph(writer, endLinear.Gradient);
                break;

            case FillStyleRadialGradient startRadial when end is FillStyleRadialGradient endRadial:
                writer.WriteUInt8((byte)FillStyleType.RadialGradient);
                startRadial.Gradient.EncodeMorph(writer, endRadial.Gradient);
                break;

            case FillStyleFocalGradient startFocal when end is FillStyleFocalGradient endFocal:
                writer.WriteUInt8((byte)FillStyleType.FocalGradient);
                startFocal.Gradient.EncodeMorph(writer, endFocal.Gradient);
                writer.WriteFixed8(startFocal.FocalPoint);
                writer.WriteFixed8(endFocal.FocalPoint);
                break;

            case FillStyleBitmap startBitmap when end is FillStyleBitmap endBitmap:
                writer.WriteUInt8((byte)(0x40 | (startBitmap.IsSmoothed ? 0 : 2) | (startBitmap.IsRepeating ? 0 : 1)));
                writer.WriteUInt16(startBitmap.Id);
                startBitmap.Matrix.Encode(writer);
                endBitmap.Matrix.Encode(writer);
                break;

            default:
                throw new NotSupportedException($"FillStyle {start} is not supported.");
        }
    }
}
