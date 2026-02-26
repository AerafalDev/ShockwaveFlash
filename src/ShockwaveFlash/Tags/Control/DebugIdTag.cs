// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.Control;

public sealed record DebugIdTag(TagMetadata Metadata, Guid Id) : Tag(Metadata)
{
    public static DebugIdTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new DebugIdTag(metadata, new Guid(reader.ReadSpan(16)));
    }
}
