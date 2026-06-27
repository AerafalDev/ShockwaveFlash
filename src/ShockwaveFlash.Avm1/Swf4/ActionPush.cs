using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Avm1.Swf4;

public sealed class ActionPush : Action
{
    public IReadOnlyList<PushValue> PushValues { get; set; }

    public ActionPush(IReadOnlyList<PushValue> pushValues) : base(ActionOpcode.Push)
    {
        PushValues = pushValues;
    }

    public static ActionPush Decode(MemoryReader reader, Avm1Context context)
    {
        var pushValues = new List<PushValue>();

        while (reader.Remaining > 0)
        {
            var type = reader.ReadUInt8();

            var pushValue = (PushValueType)type switch
            {
                PushValueType.String => PushValue.String(reader.ReadNullTerminatedString(context.Encoding)),
                PushValueType.Float => PushValue.Float(reader.ReadFloat32()),
                PushValueType.Null => PushValue.Null(),
                PushValueType.Undefined => PushValue.Undefined(),
                PushValueType.Register => PushValue.Register(reader.ReadUInt8()),
                PushValueType.Boolean => PushValue.Boolean(reader.ReadBoolean()),
                PushValueType.Double => PushValue.Double(Avm1Double.ReadDouble(reader)),
                PushValueType.Integer => PushValue.Integer(reader.ReadInt32()),
                PushValueType.Constant8 => PushValue.Constant8(reader.ReadUInt8()),
                PushValueType.Constant16 => PushValue.Constant16(reader.ReadUInt16()),
                _ => null
            };

            if (pushValue is null)
            {
                if (context.Strict)
                    throw new SwfFormatException($"Unknown AVM1 push value type 0x{type:X2}.");

                break;
            }

            pushValues.Add(pushValue);
        }

        return new ActionPush(pushValues);
    }

    public override void Encode(MemoryWriter writer, Avm1Context context)
    {
        foreach (var value in PushValues)
            EncodePushValue(writer, value, context);
    }

    private static void EncodePushValue(MemoryWriter writer, PushValue value, Avm1Context context)
    {
        switch (value)
        {
            case PushValue.PushValueString x:
                writer.WriteUInt8(0);
                writer.WriteNullTerminatedString(x.Value, context.Encoding);
                break;

            case PushValue.PushValueFloat x:
                writer.WriteUInt8(1);
                writer.WriteFloat32(x.Value);
                break;

            case PushValue.PushValueNull:
                writer.WriteUInt8(2);
                break;

            case PushValue.PushValueUndefined:
                writer.WriteUInt8(3);
                break;

            case PushValue.PushValueRegister x:
                writer.WriteUInt8(4);
                writer.WriteUInt8(x.RegisterIndex);
                break;

            case PushValue.PushValueBoolean x:
                writer.WriteUInt8(5);
                writer.WriteBoolean(x.Value);
                break;

            case PushValue.PushValueDouble x:
                writer.WriteUInt8(6);
                Avm1Double.WriteDouble(writer, x.Value);
                break;

            case PushValue.PushValueInteger x:
                writer.WriteUInt8(7);
                writer.WriteInt32(x.Value);
                break;

            case PushValue.PushValueConstant8 x:
                writer.WriteUInt8(8);
                writer.WriteUInt8(x.ConstantIndex);
                break;

            case PushValue.PushValueConstant16 x:
                writer.WriteUInt8(9);
                writer.WriteUInt16(x.ConstantIndex);
                break;

            default:
                break;
        }
    }
}
