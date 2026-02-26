// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags.Sound;

public sealed record SoundStreamBlockTag(TagMetadata Metadata, SpanSlice Data) : Tag(Metadata)
{
    public static SoundStreamBlockTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new SoundStreamBlockTag(metadata, reader.SliceToEnd());
    }
}
