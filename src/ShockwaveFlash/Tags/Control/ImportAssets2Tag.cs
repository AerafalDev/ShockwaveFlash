// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Control;

public sealed record ImportAssets2Tag(TagMetadata Metadata, string Url, AssetReference[] Assets) : Tag(Metadata)
{
    public static ImportAssets2Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var url = reader.ReadNullTerminatedString();

        reader.Advance(sizeof(ushort));

        var numAssets = reader.ReadUInt16();
        var assets = new AssetReference[numAssets];

        for (var i = 0; i < numAssets; i++)
            assets[i] = AssetReference.Decode(ref reader);

        return new ImportAssets2Tag(metadata, url, assets);
    }
}
