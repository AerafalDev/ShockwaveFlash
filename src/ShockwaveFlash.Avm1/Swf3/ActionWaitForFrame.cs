namespace ShockwaveFlash.Avm1.Swf3;

public sealed record ActionWaitForFrame(ushort Frame, byte SkipCount) : Action(ActionOpcode.WaitForFrame)
{
    public static ActionWaitForFrame Decode(MemoryReader reader)
    {
        return new ActionWaitForFrame(reader.ReadUInt16(), reader.ReadUInt8());
    }
}
