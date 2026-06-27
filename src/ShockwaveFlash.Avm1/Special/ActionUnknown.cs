namespace ShockwaveFlash.Avm1.Special;

public sealed class ActionUnknown : Action
{
    public ReadOnlyMemory<byte> Data { get; set; }

    public ActionUnknown(ActionOpcode opcode, ReadOnlyMemory<byte> data) : base(opcode)
    {
        Data = data;
    }

    public static ActionUnknown Decode(MemoryReader reader, ActionOpcode opcode)
    {
        return new ActionUnknown(opcode, reader.ReadMemoryToEnd());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteMemory(Data);
    }
}
