// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Button;

namespace ShockwaveFlash.Tags.Button;

public sealed record DefineButton2Tag(TagMetadata Metadata, ushort Id, bool IsTrackAsMenu, IReadOnlyList<ButtonRecord> Records, IReadOnlyList<ButtonAction> Actions) : Tag(Metadata)
{
    public static DefineButton2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var flags = reader.ReadUInt8();
        var isTrackAsMenu = (flags & 1) is not 0;
        var actionsOffset = reader.ReadUInt16();
        var records = new List<ButtonRecord>();

        var record = ButtonRecord.Decode(reader, 2);

        while (record is not null)
        {
            records.Add(record);
            record = ButtonRecord.Decode(reader, 2);
        }

        var actions = new List<ButtonAction>();

        if (actionsOffset is not 0)
        {
            while (true)
            {
                var (action, hasMoreActions) = ButtonAction.Decode(reader);

                actions.Add(action);

                if (!hasMoreActions)
                    break;
            }
        }

        return new DefineButton2Tag(metadata, id, isTrackAsMenu, records, actions);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt8((byte)(IsTrackAsMenu ? 1 : 0));

        var recordsWriter = new MemoryWriter();

        foreach (var record in Records)
            record.Encode(recordsWriter, 2);

        recordsWriter.WriteUInt8(0);

        var actionsOffset = Actions.Count is 0
            ? (ushort)0
            : (ushort)(2 + recordsWriter.Position);

        writer.WriteUInt16(actionsOffset);
        writer.WriteMemory(recordsWriter.WrittenMemory);

        if (Actions.Count is not 0)
            ButtonAction.EncodeCollection(writer, Actions);
    }
}
