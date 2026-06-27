using System.Text;

namespace ShockwaveFlash.Avm1.Swf5;

public sealed record ActionDefineFunction(string Name, IReadOnlyList<string> Parameters, int CodeSize) : Action(ActionOpcode.DefineFunction)
{
    public static ActionDefineFunction Decode(MemoryReader reader, Encoding encoding)
    {
        var name = reader.ReadNullTerminatedString(encoding);
        var numParams = reader.ReadUInt16();
        var parameters = new string[numParams];

        for (var i = 0; i < numParams; i++)
            parameters[i] = reader.ReadNullTerminatedString(encoding);

        var codeSize = reader.ReadUInt16();

        return new ActionDefineFunction(name, parameters, codeSize);
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteNullTerminatedString(Name, context.Encoding);
        writer.WriteUInt16((ushort)Parameters.Count);
        foreach (var parameter in Parameters)
            writer.WriteNullTerminatedString(parameter, context.Encoding);
        writer.WriteUInt16((ushort)CodeSize);
    }
}
