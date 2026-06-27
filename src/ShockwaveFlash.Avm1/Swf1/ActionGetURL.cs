using System.Text;

namespace ShockwaveFlash.Avm1.Swf1;

public sealed class ActionGetURL : Action
{
    public string Url { get; set; }

    public string Target { get; set; }

    public ActionGetURL(string url, string target) : base(ActionOpcode.GetURL)
    {
        Url = url;
        Target = target;
    }

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
