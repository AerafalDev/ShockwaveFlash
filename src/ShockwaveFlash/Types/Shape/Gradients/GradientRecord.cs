// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape.Gradients;

public sealed record GradientRecord(byte Ratio, Color Color)
{
    public static GradientRecord Decode(ref SpanReader reader, byte shapeVersion)
    {
        return new GradientRecord(reader.ReadUInt8(), shapeVersion >= 3 ? Color.DecodeRgba(ref reader) : Color.DecodeRgb(ref reader));
    }
}
