using System.Text;

namespace ShockwaveFlash.Avm1.Swf3;

public sealed record ActionGoToLabel(string Label) : Action(ActionOpcode.GoToLabel)
{
    public static ActionGoToLabel Decode(MemoryReader reader, Encoding encoding)
    {
        return new ActionGoToLabel(reader.ReadNullTerminatedString(encoding));
    }
}
