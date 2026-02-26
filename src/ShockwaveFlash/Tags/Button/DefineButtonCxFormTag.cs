// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.Button;

public sealed record DefineButtonCxFormTag(TagMetadata Metadata, ushort Id, IReadOnlyList<ColorTransform> ColorTransforms) : Tag(Metadata)
{
    public static DefineButtonCxFormTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var colorTransforms = new List<ColorTransform>();

        while (reader.Remaining > 0)
            colorTransforms.Add(ColorTransform.DecodeRgb(ref reader));

        return new DefineButtonCxFormTag(metadata, id, colorTransforms);
    }
}
