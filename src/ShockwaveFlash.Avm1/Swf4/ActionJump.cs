namespace ShockwaveFlash.Avm1.Swf4;

public sealed class ActionJump : Action
{
    public short BranchOffset { get; set; }

    public ActionJump(short branchOffset) : base(ActionOpcode.Jump)
    {
        BranchOffset = branchOffset;
    }

    public static ActionJump Decode(MemoryReader reader)
    {
        return new ActionJump(reader.ReadInt16());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteInt16(BranchOffset);
    }
}
