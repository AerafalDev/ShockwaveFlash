namespace ShockwaveFlash.Avm1.Swf4;

public sealed record ActionJump(short BranchOffset) : Action(ActionOpcode.Jump)
{
    public static ActionJump Decode(MemoryReader reader)
    {
        return new ActionJump(reader.ReadInt16());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteInt16(BranchOffset);
    }
}
