namespace ShockwaveFlash.Avm1.Swf4;

public sealed record ActionIf(short BranchOffset) : Action(ActionOpcode.If)
{
    public static ActionIf Decode(MemoryReader reader)
    {
        return new ActionIf(reader.ReadInt16());
    }
}
