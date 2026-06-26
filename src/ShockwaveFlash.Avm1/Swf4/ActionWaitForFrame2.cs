namespace ShockwaveFlash.Avm1.Swf4;

public sealed record ActionWaitForFrame2(byte SkipCount) : Action(ActionOpcode.WaitForFrame2)
{
    public static ActionWaitForFrame2 Decode(MemoryReader reader)
    {
        return new ActionWaitForFrame2(reader.ReadUInt8());
    }
}
