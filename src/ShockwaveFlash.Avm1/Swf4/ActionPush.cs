using System.Text;
using ShockwaveFlash.Avm1.Types;

namespace ShockwaveFlash.Avm1.Swf4;

public sealed record ActionPush(IReadOnlyList<PushValue> PushValues) : Action(ActionOpcode.Push)
{
    public static ActionPush Decode(MemoryReader reader, Encoding encoding)
    {
        var pushValues = new List<PushValue>();

        while (reader.Remaining > 0)
        {
            var type = reader.ReadUInt8();

            var pushValue = (PushValueType)type switch
            {
                PushValueType.String => PushValue.String(reader.ReadNullTerminatedString(encoding)),
                PushValueType.Float => PushValue.Float(reader.ReadFloat32()),
                PushValueType.Null => PushValue.Null(),
                PushValueType.Undefined => PushValue.Undefined(),
                PushValueType.Register => PushValue.Register(reader.ReadUInt8()),
                PushValueType.Boolean => PushValue.Boolean(reader.ReadBoolean()),
                PushValueType.Double => PushValue.Double(Avm1Number.ReadDouble(reader)),
                PushValueType.Integer => PushValue.Integer(reader.ReadInt32()),
                PushValueType.Constant8 => PushValue.Constant8(reader.ReadUInt8()),
                PushValueType.Constant16 => PushValue.Constant16(reader.ReadUInt16()),
                _ => throw new ArgumentOutOfRangeException($"Push type 0x{type:X2} is not supported.")
            };

            pushValues.Add(pushValue);
        }

        return new ActionPush(pushValues);
    }
}
