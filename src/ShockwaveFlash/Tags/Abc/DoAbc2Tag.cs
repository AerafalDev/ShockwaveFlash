// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;
using ShockwaveFlash.Types.Abc;

namespace ShockwaveFlash.Tags.Abc;

public sealed record DoAbc2Tag(TagMetadata Metadata, DoAbc2Flags Flags, string Name, SpanSlice Data) : Tag(Metadata)
{
    public static DoAbc2Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var flags = (DoAbc2Flags)reader.ReadUInt32();
        var name = reader.ReadNullTerminatedString();
        var data = reader.SliceToEnd();

        return new DoAbc2Tag(metadata, flags, name, data);
    }
}
