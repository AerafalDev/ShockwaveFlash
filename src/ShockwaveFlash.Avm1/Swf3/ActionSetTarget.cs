using System.Text;

namespace ShockwaveFlash.Avm1.Swf3;

public sealed class ActionSetTarget : Action
{
    public string TargetName { get; set; }

    public ActionSetTarget(string targetName) : base(ActionOpcode.SetTarget)
    {
        TargetName = targetName;
    }

    public static ActionSetTarget Decode(MemoryReader reader, Encoding encoding)
    {
        return new ActionSetTarget(reader.ReadNullTerminatedString(encoding));
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteNullTerminatedString(TargetName, context.Encoding);
    }
}
