// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed record StartSound2Tag(TagMetadata Metadata, string ClassName, SoundInfo SoundInfo) : Tag(Metadata)
{
    public static StartSound2Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var className = reader.ReadNullTerminatedString();
        var soundInfo = SoundInfo.Decode(ref reader);

        return new StartSound2Tag(metadata, className, soundInfo);
    }
}
