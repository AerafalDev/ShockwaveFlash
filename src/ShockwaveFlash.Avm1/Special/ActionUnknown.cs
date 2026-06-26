namespace ShockwaveFlash.Avm1.Special;

public sealed record ActionUnknown(ActionOpcode Opcode, ReadOnlyMemory<byte> Data) : Action(Opcode)
{
    public static ActionUnknown Decode(MemoryReader reader, ActionOpcode opcode)
    {
        return new ActionUnknown(opcode, reader.ReadMemoryToEnd());
    }
}
