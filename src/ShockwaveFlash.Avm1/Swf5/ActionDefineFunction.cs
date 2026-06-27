namespace ShockwaveFlash.Avm1.Swf5;

public sealed record ActionDefineFunction(string Name, IReadOnlyList<string> Parameters, ReadOnlyMemory<byte> Body) : Action(ActionOpcode.DefineFunction)
{
    public static ActionDefineFunction Decode(MemoryReader header, MemoryReader outer, Avm1Context context)
    {
        var name = header.ReadNullTerminatedString(context.Encoding);
        var numParams = header.ReadUInt16();
        var parameters = new string[numParams];

        for (var i = 0; i < numParams; i++)
            parameters[i] = header.ReadNullTerminatedString(context.Encoding);

        var codeSize = header.ReadUInt16();

        return new ActionDefineFunction(name, parameters, outer.ReadMemory(codeSize));
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteNullTerminatedString(Name, context.Encoding);
        writer.WriteUInt16((ushort)Parameters.Count);
        foreach (var parameter in Parameters)
            writer.WriteNullTerminatedString(parameter, context.Encoding);
        writer.WriteUInt16((ushort)Body.Length);
    }

    public override void EncodeTrailer(MemoryWriter writer)
    {
        writer.WriteMemory(Body);
    }
}
