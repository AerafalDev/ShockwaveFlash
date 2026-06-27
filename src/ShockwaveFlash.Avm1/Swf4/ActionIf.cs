namespace ShockwaveFlash.Avm1.Swf4;

public sealed class ActionIf : Action
{
    public short BranchOffset { get; set; }

    public ActionIf(short branchOffset) : base(ActionOpcode.If)
    {
        BranchOffset = branchOffset;
    }

    public static ActionIf Decode(MemoryReader reader)
    {
        return new ActionIf(reader.ReadInt16());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteInt16(BranchOffset);
    }
}
