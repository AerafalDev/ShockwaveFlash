// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Button;

namespace ShockwaveFlash.Tags;

public sealed record DefineButton2Tag(TagMetadata Metadata, ushort Id, bool IsTrackAsMenu, IReadOnlyList<ButtonRecord> Records, IReadOnlyList<ButtonAction> Actions) : Tag(Metadata)
{
    public static DefineButton2Tag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var flags = reader.ReadUInt8();
        var isTrackAsMenu = (flags & 1) is not 0;
        var actionsOffset = reader.ReadUInt16();
        var records = new List<ButtonRecord>();

        var record = ButtonRecord.Decode(ref reader, 2);

        while (record is not null)
        {
            records.Add(record);
            record = ButtonRecord.Decode(ref reader, 2);
        }

        var actions = new List<ButtonAction>();

        if (actionsOffset is not 0)
        {
            while (true)
            {
                var (action, hasMoreActions) = ButtonAction.Decode(ref reader);

                actions.Add(action);

                if (!hasMoreActions)
                    break;
            }
        }

        return new DefineButton2Tag(metadata, id, isTrackAsMenu, records, actions);
    }
}
