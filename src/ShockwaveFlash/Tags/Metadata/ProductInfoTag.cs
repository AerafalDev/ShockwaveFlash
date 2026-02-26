// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Metadata;

public sealed record ProductInfoTag(TagMetadata Metadata, FlashProduct ProductId, FlashEdition Edition, byte MajorVersion, byte MinorVersion, uint BuildLow, uint BuildHigh, DateTime CompilationDate) : Tag(Metadata)
{
    public static ProductInfoTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var productId = (FlashProduct)reader.ReadUInt32();
        var edition = (FlashEdition)reader.ReadUInt32();
        var majorVersion = reader.ReadUInt8();
        var minorVersion = reader.ReadUInt8();
        var buildLow = reader.ReadUInt32();
        var buildHigh = reader.ReadUInt32();
        var compilationDate = DateTime.UnixEpoch.AddMilliseconds(reader.ReadUInt64());

        return new ProductInfoTag(metadata, productId, edition, majorVersion, minorVersion, buildLow, buildHigh, compilationDate);
    }
}
