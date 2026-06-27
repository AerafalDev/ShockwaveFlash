using System.Text;

namespace ShockwaveFlash.Avm1.Swf3;

public sealed class ActionGoToLabel : Action
{
    public string Label { get; set; }

    public ActionGoToLabel(string label) : base(ActionOpcode.GoToLabel)
    {
        Label = label;
    }

    public static ActionGoToLabel Decode(MemoryReader reader, Encoding encoding)
    {
        return new ActionGoToLabel(reader.ReadNullTerminatedString(encoding));
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteNullTerminatedString(Label, context.Encoding);
    }
}
