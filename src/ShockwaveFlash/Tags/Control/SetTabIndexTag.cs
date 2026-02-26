// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.Control;

public sealed record SetTabIndexTag(TagMetadata Metadata, ushort Depth, ushort TabIndex) : Tag(Metadata)
{
    public static SetTabIndexTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new SetTabIndexTag(metadata, reader.ReadUInt16(), reader.ReadUInt16());
    }
}
