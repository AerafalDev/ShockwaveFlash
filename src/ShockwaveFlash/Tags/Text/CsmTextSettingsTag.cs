// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Text;

namespace ShockwaveFlash.Tags.Text;

public sealed record CsmTextSettingsTag(TagMetadata Metadata, ushort Id, bool UseAdvancedRendering, TextGridFit GridFit, float Thickness, float Sharpness) : Tag(Metadata)
{
    public static CsmTextSettingsTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var flags = reader.ReadUInt8();
        var useAdvancedRendering = (flags & 64) is not 0;
        var gridFit = (TextGridFit)((flags >> 3) & 3);
        var thickness = reader.ReadFloat32();
        var sharpness = reader.ReadFloat32();

        reader.Advance(sizeof(byte));

        return new CsmTextSettingsTag(metadata, id, useAdvancedRendering, gridFit, thickness, sharpness);
    }
}
