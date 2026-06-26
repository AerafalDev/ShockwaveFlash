using ShockwaveFlash.Types.Button;

namespace ShockwaveFlash.Tags.Button;

public sealed class DefineButtonTag : Tag
{
    public ushort Id { get; set; }

    public IReadOnlyList<ButtonRecord> Records { get; set; }

    public IReadOnlyList<ButtonAction> Actions { get; set; }

    public DefineButtonTag(TagMetadata metadata, ushort id, IReadOnlyList<ButtonRecord> records, IReadOnlyList<ButtonAction> actions) : base(metadata)
    {
        Id = id;
        Records = records;
        Actions = actions;
    }

    public static DefineButtonTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var records = new List<ButtonRecord>();

        var record = ButtonRecord.Decode(reader, 1);

        while (record is not null)
        {
            records.Add(record);
            record = ButtonRecord.Decode(reader, 1);
        }

        var actions = reader.ReadMemoryToEnd();

        return new DefineButtonTag(metadata, id, records, [new ButtonAction(ButtonActionCondition.OverDownToOverUp, actions)]);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);

        foreach (var record in Records)
            record.Encode(writer, 1);

        writer.WriteUInt8(0);

        foreach (var action in Actions)
            writer.WriteMemory(action.Data);
    }
}
