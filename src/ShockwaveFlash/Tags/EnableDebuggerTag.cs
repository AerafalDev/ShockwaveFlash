// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags;

public sealed record EnableDebuggerTag(TagMetadata Metadata, string Password) : Tag(Metadata)
{
    public static EnableDebuggerTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new EnableDebuggerTag(metadata, reader.ReadNullTerminatedString());
    }
}
