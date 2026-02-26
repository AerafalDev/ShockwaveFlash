// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;

namespace ShockwaveFlash.Types.Filter;

public sealed record BevelFilter(Color ShadowColor, Color HighlightColor, Vector2 Blur, float Angle, float Distance, float Strength, BevelFilterFlags Flags) : Filter
{
    public bool IsInner =>
        Flags.HasFlag(BevelFilterFlags.InnerShadow);

    public bool IsKnockout =>
        Flags.HasFlag(BevelFilterFlags.Knockout);

    public bool IsOnTop =>
        Flags.HasFlag(BevelFilterFlags.OnTop);

    public byte Passes =>
        (byte)(Flags & BevelFilterFlags.Passes);

    public BlurFilter GetInnerBlurFilter()
    {
        return new BlurFilter(Blur, BlurFilterFlags.FromPasses(Passes));
    }

    public static new BevelFilter Decode(ref SpanReader reader)
    {
        var highlightColor = Color.DecodeRgba(ref reader);
        var shadowColor = Color.DecodeRgba(ref reader);
        var blur = reader.ReadVector2();
        var angle = reader.ReadFixed();
        var distance = reader.ReadFixed();
        var strength = reader.ReadFixed8();
        var flags = (BevelFilterFlags)reader.ReadUInt8();

        return new BevelFilter(shadowColor, highlightColor, blur, angle, distance, strength, flags);
    }
}
