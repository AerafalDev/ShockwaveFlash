// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Tags.Sound;

public sealed record SoundStreamBlockTag(TagMetadata Metadata, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static SoundStreamBlockTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new SoundStreamBlockTag(metadata, reader.ReadMemoryToEnd());
    }
}
