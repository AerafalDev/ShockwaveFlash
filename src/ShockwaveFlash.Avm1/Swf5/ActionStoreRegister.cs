namespace ShockwaveFlash.Avm1.Swf5;

public sealed class ActionStoreRegister : Action
{
    public byte RegisterNumber { get; set; }

    public ActionStoreRegister(byte registerNumber) : base(ActionOpcode.StoreRegister)
    {
        RegisterNumber = registerNumber;
    }

    public static ActionStoreRegister Decode(MemoryReader reader)
    {
        return new ActionStoreRegister(reader.ReadUInt8());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt8(RegisterNumber);
    }
}
