// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Filter;

public abstract record Filter
{
    public static Filter Decode(MemoryReader reader)
    {
        var type = reader.ReadUInt8();

        return (FilterType)type switch
        {
            FilterType.DropShadowFilter => DropShadowFilter.Decode(reader),
            FilterType.BlurFilter => BlurFilter.Decode(reader),
            FilterType.GlowFilter => GlowFilter.Decode(reader),
            FilterType.BevelFilter => BevelFilter.Decode(reader),
            FilterType.GradientGlowFilter => GradientFilter.Decode(reader),
            FilterType.ConvolutionFilter => ConvolutionFilter.Decode(reader),
            FilterType.ColorMatrixFilter => ColorMatrixFilter.Decode(reader),
            FilterType.GradientBevelFilter => GradientFilter.Decode(reader),
            _ => throw new NotSupportedException($"Filter type {type} is not supported.")
        };
    }

    public void Encode(MemoryWriter writer)
    {
        switch (this)
        {
            case DropShadowFilter dropShadowFilter:
                writer.WriteUInt8((byte)FilterType.DropShadowFilter);
                dropShadowFilter.Encode(writer);
                break;
            case BlurFilter blurFilter:
                writer.WriteUInt8((byte)FilterType.BlurFilter);
                blurFilter.Encode(writer);
                break;
            case GlowFilter glowFilter:
                writer.WriteUInt8((byte)FilterType.GlowFilter);
                glowFilter.Encode(writer);
                break;
            case BevelFilter bevelFilter:
                writer.WriteUInt8((byte)FilterType.BevelFilter);
                bevelFilter.Encode(writer);
                break;
            case GradientFilter gradientFilter:
                writer.WriteUInt8((byte)FilterType.GradientGlowFilter);
                gradientFilter.Encode(writer);
                break;
            case ConvolutionFilter convolutionFilter:
                writer.WriteUInt8((byte)FilterType.ConvolutionFilter);
                convolutionFilter.Encode(writer);
                break;
            case ColorMatrixFilter colorMatrixFilter:
                writer.WriteUInt8((byte)FilterType.ColorMatrixFilter);
                colorMatrixFilter.Encode(writer);
                break;
            default:
                throw new NotSupportedException($"Filter type {GetType()} is not supported.");
        }
    }
}
