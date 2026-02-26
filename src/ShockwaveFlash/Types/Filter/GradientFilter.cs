// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;
using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Filter;

public sealed record GradientFilter(GradientRecord[] Colors, Vector2 Blur, float Angle, float Distance, float Strength, GradientFilterFlags Flags) : Filter
{
    public bool IsInner =>
        Flags.HasFlag(GradientFilterFlags.InnerShadow);

    public bool IsKnockout =>
        Flags.HasFlag(GradientFilterFlags.Knockout);

    public bool IsOnTop =>
        Flags.HasFlag(GradientFilterFlags.OnTop);

    public bool IsCompositeSource =>
        Flags.HasFlag(GradientFilterFlags.CompositeSource);

    public byte Passes =>
        (byte)(Flags & GradientFilterFlags.Passes);

    public BlurFilter GetInnerBlurFilter()
    {
        return new BlurFilter(Blur, BlurFilterFlags.FromPasses(Passes));
    }

    public static new GradientFilter Decode(ref SpanReader reader)
    {
        var numColors = reader.ReadUInt8();
        var colors = new Color[numColors];

        for (var i = 0; i < numColors; i++)
            colors[i] = Color.DecodeRgba(ref reader);

        var records = new GradientRecord[numColors];

        for (var i = 0; i < numColors; i++)
            records[i] = new GradientRecord(reader.ReadUInt8(), colors[i]);

        var blur = reader.ReadVector2();
        var angle = reader.ReadFixed();
        var distance = reader.ReadFixed();
        var strength = reader.ReadFixed8();
        var flags = (GradientFilterFlags)reader.ReadUInt8();

        return new GradientFilter(records, blur, angle, distance, strength, flags);
    }
}
