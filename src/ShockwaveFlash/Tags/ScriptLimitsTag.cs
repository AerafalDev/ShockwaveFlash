// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags;

public sealed record ScriptLimitsTag(TagMetadata Metadata, ushort MaxRecursionDepth, TimeSpan Timeout) : Tag(Metadata)
{
    public static ScriptLimitsTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new ScriptLimitsTag(metadata, reader.ReadUInt16(), TimeSpan.FromSeconds(reader.ReadUInt16()));
    }
}
