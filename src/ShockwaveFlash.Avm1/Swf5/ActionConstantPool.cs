using System.Text;

namespace ShockwaveFlash.Avm1.Swf5;

public sealed record ActionConstantPool(IReadOnlyList<string> Constants) : Action(ActionOpcode.ConstantPool)
{
    public static ActionConstantPool Decode(MemoryReader reader, Encoding encoding)
    {
        var count = reader.ReadUInt16();
        var constants = new string[count];

        for (var i = 0; i < count; i++)
            constants[i] = reader.ReadNullTerminatedString(encoding);

        return new ActionConstantPool(constants);
    }
}
