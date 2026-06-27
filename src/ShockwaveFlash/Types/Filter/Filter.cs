using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Types.Filter;

public abstract class Filter
{
    public static Filter Decode(MemoryReader reader)
    {
        var type = reader.ReadUInt8();

        return (FilterType)type switch
        {
            FilterType.DropShadowFilter => DropShadowFilter.DecodeBody(reader),
            FilterType.BlurFilter => BlurFilter.DecodeBody(reader),
            FilterType.GlowFilter => GlowFilter.DecodeBody(reader),
            FilterType.BevelFilter => BevelFilter.DecodeBody(reader),
            FilterType.GradientGlowFilter => GradientFilter.DecodeBody(reader, FilterType.GradientGlowFilter),
            FilterType.ConvolutionFilter => ConvolutionFilter.DecodeBody(reader),
            FilterType.ColorMatrixFilter => ColorMatrixFilter.DecodeBody(reader),
            FilterType.GradientBevelFilter => GradientFilter.DecodeBody(reader, FilterType.GradientBevelFilter),
            _ => throw new SwfFormatException($"Unknown filter type {type}.")
        };
    }

    public void Encode(MemoryWriter writer)
    {
        switch (this)
        {
            case DropShadowFilter dropShadowFilter:
                writer.WriteUInt8((byte)FilterType.DropShadowFilter);
                dropShadowFilter.EncodeBody(writer);
                break;
            case BlurFilter blurFilter:
                writer.WriteUInt8((byte)FilterType.BlurFilter);
                blurFilter.EncodeBody(writer);
                break;
            case GlowFilter glowFilter:
                writer.WriteUInt8((byte)FilterType.GlowFilter);
                glowFilter.EncodeBody(writer);
                break;
            case BevelFilter bevelFilter:
                writer.WriteUInt8((byte)FilterType.BevelFilter);
                bevelFilter.EncodeBody(writer);
                break;
            case GradientFilter gradientFilter:
                writer.WriteUInt8((byte)gradientFilter.Type);
                gradientFilter.EncodeBody(writer);
                break;
            case ConvolutionFilter convolutionFilter:
                writer.WriteUInt8((byte)FilterType.ConvolutionFilter);
                convolutionFilter.EncodeBody(writer);
                break;
            case ColorMatrixFilter colorMatrixFilter:
                writer.WriteUInt8((byte)FilterType.ColorMatrixFilter);
                colorMatrixFilter.EncodeBody(writer);
                break;
            default:
                throw new SwfUnsupportedException($"Filter type {GetType()} is not supported.");
        }
    }
}
