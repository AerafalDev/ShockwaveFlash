// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags.Metadata;

public sealed record EnableTelemetryTag(TagMetadata Metadata, SpanSlice PasswordHash) : Tag(Metadata)
{
    public static EnableTelemetryTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        reader.Advance(sizeof(ushort));

        return new EnableTelemetryTag(metadata, metadata.Length > 2 ? reader.Slice(32) : SpanSlice.Empty);
    }
}
