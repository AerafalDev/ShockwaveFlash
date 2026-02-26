// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags.Abc;

public sealed record DoAbcTag(TagMetadata Metadata, SpanSlice Data) : Tag(Metadata)
{
    public static DoAbcTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new DoAbcTag(metadata, reader.SliceToEnd());
    }
}
