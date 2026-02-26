// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Font;

namespace ShockwaveFlash.Tags;

public sealed record DefineFontAlignZonesTag(TagMetadata Metadata, ushort Id, FontThickness Thickness, IReadOnlyList<FontAlignZone> Zones) : Tag(Metadata)
{
    public static DefineFontAlignZonesTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var thickness = (FontThickness)(reader.ReadUInt8() >> 6);
        var zones = new List<FontAlignZone>();

        while (reader.Remaining > 0)
            zones.Add(FontAlignZone.Decode(ref reader));

        return new DefineFontAlignZonesTag(metadata, id, thickness, zones);
    }
}
