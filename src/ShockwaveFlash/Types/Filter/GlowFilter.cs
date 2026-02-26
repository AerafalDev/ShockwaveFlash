// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;

namespace ShockwaveFlash.Types.Filter;

public sealed record GlowFilter(Color Color, Vector2 Blur, float Strength, GlowFilterFlags Flags) : Filter
{
    public bool IsInner =>
        Flags.HasFlag(GlowFilterFlags.InnerGlow);

    public bool IsKnockout =>
        Flags.HasFlag(GlowFilterFlags.Knockout);

    public bool IsCompositeSource =>
        Flags.HasFlag(GlowFilterFlags.CompositeSource);

    public byte Passes =>
        (byte)(Flags & GlowFilterFlags.Passes);

    public BlurFilter GetInnerBlurFilter()
    {
        return new BlurFilter(Blur, BlurFilterFlags.FromPasses(Passes));
    }

    public static new GlowFilter Decode(ref SpanReader reader)
    {
        var color = Color.DecodeRgba(ref reader);
        var blur = reader.ReadVector2();
        var strength = reader.ReadFixed8();
        var flags = (GlowFilterFlags)reader.ReadUInt8();

        return new GlowFilter(color, blur, strength, flags);
    }
}
