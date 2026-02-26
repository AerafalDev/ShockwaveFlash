// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags;

public sealed record SetBackgroundColorTag(TagMetadata Metadata, Color BackgroundColor) : Tag(Metadata)
{
    public static SetBackgroundColorTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new SetBackgroundColorTag(metadata, Color.DecodeRgb(ref reader));
    }
}
