// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags;

public sealed record FrameLabelTag(TagMetadata Metadata, string Name, bool IsAnchor) : Tag(Metadata)
{
    public static FrameLabelTag Decode(ref SpanReader reader, TagMetadata metadata, byte swfVersion)
    {
        var name = reader.ReadNullTerminatedString();
        var isAnchor = false;

        if (swfVersion >= 6 && reader.Remaining > 0)
            isAnchor = reader.ReadBoolean();

        return new FrameLabelTag(metadata, name, isAnchor);
    }
}
