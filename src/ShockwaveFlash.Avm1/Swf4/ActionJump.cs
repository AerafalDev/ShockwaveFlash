namespace ShockwaveFlash.Avm1.Swf4;

public sealed record ActionJump(short BranchOffset) : Action(ActionOpcode.Jump)
{
    public static ActionJump Decode(MemoryReader reader)
    {
        return new ActionJump(reader.ReadInt16());
    }
}
