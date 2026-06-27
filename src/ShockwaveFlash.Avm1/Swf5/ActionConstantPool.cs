using System.Text;

namespace ShockwaveFlash.Avm1.Swf5;

public sealed class ActionConstantPool : Action
{
    public IReadOnlyList<string> Constants { get; set; }

    public ActionConstantPool(IReadOnlyList<string> constants) : base(ActionOpcode.ConstantPool)
    {
        Constants = constants;
    }

    public static ActionConstantPool Decode(MemoryReader reader, Encoding encoding)
    {
        var count = reader.ReadUInt16();
        var constants = new string[count];

        for (var i = 0; i < count; i++)
            constants[i] = reader.ReadNullTerminatedString(encoding);

        return new ActionConstantPool(constants);
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        writer.WriteUInt16((ushort)Constants.Count);
        foreach (var constant in Constants)
            writer.WriteNullTerminatedString(constant, context.Encoding);
    }
}
