// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags;

public sealed record PlaceObjectTag(TagMetadata Metadata, ushort Id, ushort Depth, Matrix Matrix, ColorTransform? ColorTransform) : Tag(Metadata)
{
    public static PlaceObjectTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var depth = reader.ReadUInt16();
        var matrix = Matrix.Decode(ref reader);

        ColorTransform? colorTransform = reader.Remaining > 0
            ? Types.ColorTransform.DecodeRgb(ref reader)
            : null;

        return new PlaceObjectTag(metadata, id, depth, matrix, colorTransform);
    }
}
