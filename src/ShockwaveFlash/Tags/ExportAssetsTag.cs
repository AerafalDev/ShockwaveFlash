// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags;

public sealed record ExportAssetsTag(TagMetadata Metadata, AssetReference[] Assets) : Tag(Metadata)
{
    public static ExportAssetsTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var assetsCount = reader.ReadUInt16();
        var assets = new AssetReference[assetsCount];

        for (var i = 0; i < assets.Length; i++)
            assets[i] = AssetReference.Decode(ref reader);

        return new ExportAssetsTag(metadata, assets);
    }
}
