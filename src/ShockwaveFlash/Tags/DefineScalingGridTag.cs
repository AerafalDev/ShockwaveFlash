// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;

namespace ShockwaveFlash.Tags;

public sealed record DefineScalingGridTag(TagMetadata Metadata, ushort Id, Rectangle Splitter) : Tag(Metadata)
{
    public static DefineScalingGridTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new DefineScalingGridTag(metadata, reader.ReadUInt16(), Rectangle.Decode(ref reader));
    }
}
