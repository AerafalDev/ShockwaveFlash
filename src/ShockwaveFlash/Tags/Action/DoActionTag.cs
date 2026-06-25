// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tags.Action;

public sealed record DoActionTag(TagMetadata Metadata, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public IReadOnlyList<Actions.Avm1.Action> DecodeActions(byte swfVersion)
    {
        return Actions.Avm1.Action.DecodeCollection(Data, swfVersion);
    }

    public static DoActionTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new DoActionTag(metadata, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer)
    {
        writer.WriteMemory(Data);
    }
}
