namespace ShockwaveFlash.Avm1.Swf5;

public sealed record ActionStoreRegister(byte RegisterNumber) : Action(ActionOpcode.StoreRegister)
{
    public static ActionStoreRegister Decode(MemoryReader reader)
    {
        return new ActionStoreRegister(reader.ReadUInt8());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt8(RegisterNumber);
    }
}
