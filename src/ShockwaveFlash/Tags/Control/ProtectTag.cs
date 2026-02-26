// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.Control;

public sealed record ProtectTag(TagMetadata Metadata, string? PasswordHash) : Tag(Metadata)
{
    public static ProtectTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new ProtectTag(metadata, metadata.Length > 0 ? reader.ReadNullTerminatedString() : null);
    }
}
