// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace ShockwaveFlash.Tags.Metadata;

public sealed record EnableTelemetryTag(TagMetadata Metadata, ReadOnlyMemory<byte> PasswordHash) : Tag(Metadata)
{
    public static EnableTelemetryTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        reader.Advance(sizeof(ushort));

        return new EnableTelemetryTag(metadata, metadata.Length > 2 ? reader.ReadMemory(32) : ReadOnlyMemory<byte>.Empty);
    }
}
