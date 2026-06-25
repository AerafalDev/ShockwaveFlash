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
}
