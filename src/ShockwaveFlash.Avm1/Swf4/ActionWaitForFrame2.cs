namespace ShockwaveFlash.Avm1.Swf4;

public sealed class ActionWaitForFrame2 : Action
{
    public byte SkipCount { get; set; }

    public ActionWaitForFrame2(byte skipCount) : base(ActionOpcode.WaitForFrame2)
    {
        SkipCount = skipCount;
    }

    public static ActionWaitForFrame2 Decode(MemoryReader reader)
    {
        return new ActionWaitForFrame2(reader.ReadUInt8());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt8(SkipCount);
    }
}
