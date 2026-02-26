// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Control;

public sealed record ImportAssetsTag(TagMetadata Metadata, string Url, AssetReference[] Assets) : Tag(Metadata)
{
    public static ImportAssetsTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var url = reader.ReadNullTerminatedString();
        var numAssets = reader.ReadUInt16();
        var assets = new AssetReference[numAssets];

        for (var i = 0; i < numAssets; i++)
            assets[i] = AssetReference.Decode(ref reader);

        return new ImportAssetsTag(metadata, url, assets);
    }
}
