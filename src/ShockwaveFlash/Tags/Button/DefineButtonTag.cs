// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Button;

namespace ShockwaveFlash.Tags.Button;

public sealed record DefineButtonTag(TagMetadata Metadata, ushort Id, IReadOnlyList<ButtonRecord> Records, IReadOnlyList<ButtonAction> Actions) : Tag(Metadata)
{
    public static DefineButtonTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var records = new List<ButtonRecord>();

        var record = ButtonRecord.Decode(ref reader, 1);

        while (record is not null)
        {
            records.Add(record);
            record = ButtonRecord.Decode(ref reader, 1);
        }

        var actions = reader.SliceToEnd();

        return new DefineButtonTag(metadata, id, records, [new ButtonAction(ButtonActionCondition.OverDownToOverUp, actions)]);
    }
}
