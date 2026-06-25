// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags.DisplayList;

public sealed record PlaceObjectTag(TagMetadata Metadata, ushort Id, ushort Depth, Matrix Matrix, ColorTransform? ColorTransform) : Tag(Metadata)
{
    public static PlaceObjectTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var depth = reader.ReadUInt16();
        var matrix = Matrix.Decode(reader);

        ColorTransform? colorTransform = reader.Remaining > 0
            ? Types.ColorTransform.DecodeRgb(reader)
            : null;

        return new PlaceObjectTag(metadata, id, depth, matrix, colorTransform);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt16(Depth);
        Matrix.Encode(writer);

        if (ColorTransform is { } colorTransform)
            colorTransform.EncodeRgb(writer);
    }
}
