using System.Text;

namespace ShockwaveFlash.Avm1.Swf1;

public sealed record ActionGetURL(string Url, string Target) : Action(ActionOpcode.GetURL)
{
    public static ActionGetURL Decode(MemoryReader reader, Encoding encoding)
    {
        return new ActionGetURL(reader.ReadNullTerminatedString(encoding), reader.ReadNullTerminatedString(encoding));
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteNullTerminatedString(Url, context.Encoding);
        writer.WriteNullTerminatedString(Target, context.Encoding);
    }
}
