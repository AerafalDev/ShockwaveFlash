// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Filter;

public abstract record Filter
{
    public static Filter Decode(ref SpanReader reader)
    {
        var type = reader.ReadUInt8();

        return (FilterType)type switch
        {
            FilterType.DropShadowFilter => DropShadowFilter.Decode(ref reader),
            FilterType.BlurFilter => BlurFilter.Decode(ref reader),
            FilterType.GlowFilter => GlowFilter.Decode(ref reader),
            FilterType.BevelFilter => BevelFilter.Decode(ref reader),
            FilterType.GradientGlowFilter => GradientFilter.Decode(ref reader),
            FilterType.ConvolutionFilter => ConvolutionFilter.Decode(ref reader),
            FilterType.ColorMatrixFilter => ColorMatrixFilter.Decode(ref reader),
            FilterType.GradientBevelFilter => GradientFilter.Decode(ref reader),
            _ => throw new NotSupportedException($"Filter type {type} is not supported.")
        };
    }
}
