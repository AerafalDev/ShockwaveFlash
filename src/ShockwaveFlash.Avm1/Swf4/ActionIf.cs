namespace ShockwaveFlash.Avm1.Swf4;

public sealed record ActionIf(short BranchOffset) : Action(ActionOpcode.If)
{
    public static ActionIf Decode(MemoryReader reader)
    {
        return new ActionIf(reader.ReadInt16());
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteInt16(BranchOffset);
    }
}
