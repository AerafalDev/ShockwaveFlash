// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.IO.Buffers;

namespace ShockwaveFlash.Tags.Action;

public sealed record DoInitActionTag(TagMetadata Metadata, ushort Id, SpanSlice Data) : Tag(Metadata)
{
    public IReadOnlyList<Actions.Avm1.Action> DecodeActions(ref SpanReader reader, byte swfVersion)
    {
        return Actions.Avm1.Action.DecodeCollection(reader.ReadSlice(Data), swfVersion);
    }

    public static DoInitActionTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new DoInitActionTag(metadata, reader.ReadUInt16(), reader.SliceToEnd());
    }
}
