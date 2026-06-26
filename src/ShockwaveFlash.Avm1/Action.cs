using System.Text;
using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf1;
using ShockwaveFlash.Avm1.Swf2;
using ShockwaveFlash.Avm1.Swf3;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Swf6;
using ShockwaveFlash.Avm1.Swf7;
using ShockwaveFlash.Avm1.Types;
using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Avm1;

public abstract record Action(ActionOpcode Opcode)
{
    public static IReadOnlyList<Action> DecodeCollection(ReadOnlyMemory<byte> buffer, byte swfVersion)
    {
        var encoding = swfVersion >= 6 ? Encoding.UTF8 : Encoding.GetEncoding("iso-8859-1");
        var actions = new List<Action>(capacity: 64);
        var reader = new MemoryReader(buffer);

        while (reader.Remaining > 0)
        {
            var opcodeRaw = reader.ReadUInt8();
            var opcode = (ActionOpcode)opcodeRaw;

            var payloadLength = 0;

            if (opcodeRaw >= 128)
                payloadLength = reader.ReadUInt16();

            var actionReader = new MemoryReader(reader.ReadMemory(payloadLength));
            var action = Decode(actionReader, opcode, encoding);

            if (actionReader.Remaining > 0)
                throw new InvalidOperationException($"Action length mismatch. Expected {payloadLength} bytes, got {actionReader.Position}.");

            actions.Add(action);

            if (action is ActionEnd)
                break;
        }

        return actions;
    }

    public static ReadOnlyMemory<byte> EncodeCollection(IReadOnlyList<Action> actions, byte swfVersion)
    {
        var encoding = swfVersion >= 6 ? Encoding.UTF8 : Encoding.GetEncoding("iso-8859-1");
        var writer = new MemoryWriter();
        var body = new MemoryWriter();

        foreach (var action in actions)
        {
            var opcode = (byte)action.Opcode;
            writer.WriteUInt8(opcode);

            if (opcode < 128)
                continue;

            body.Reset();
            EncodeBody(action, body, encoding);

            if (body.Position > ushort.MaxValue)
                throw new SwfFormatException($"AVM1 action {action.Opcode} body of {body.Position} bytes exceeds the 65535-byte action limit.");

            writer.WriteUInt16((ushort)body.Position);
            writer.WriteMemory(body.WrittenMemory);
        }

        return writer.WrittenMemory;
    }

    private static void EncodeBody(Action action, MemoryWriter writer, Encoding encoding)
    {
        switch (action)
        {
            case ActionGotoFrame x:
                writer.WriteUInt16(x.Frame);
                break;

            case ActionGetURL x:
                writer.WriteNullTerminatedString(x.Url, encoding);
                writer.WriteNullTerminatedString(x.Target, encoding);
                break;

            case ActionGetURL2 x:
                writer.WriteUInt8((byte)x.Flags);
                break;

            case ActionStoreRegister x:
                writer.WriteUInt8(x.RegisterNumber);
                break;

            case ActionWaitForFrame x:
                writer.WriteUInt16(x.Frame);
                writer.WriteUInt8(x.SkipCount);
                break;

            case ActionWaitForFrame2 x:
                writer.WriteUInt8(x.SkipCount);
                break;

            case ActionSetTarget x:
                writer.WriteNullTerminatedString(x.TargetName, encoding);
                break;

            case ActionGoToLabel x:
                writer.WriteNullTerminatedString(x.Label, encoding);
                break;

            case ActionJump x:
                writer.WriteInt16(x.BranchOffset);
                break;

            case ActionIf x:
                writer.WriteInt16(x.BranchOffset);
                break;

            case ActionGotoFrame2 x:
                writer.WriteUInt8((byte)((x.Play ? 1 : 0) | (x.HasSceneBias ? 2 : 0)));
                if (x.HasSceneBias)
                    writer.WriteUInt16(x.SceneBias);
                break;

            case ActionWith x:
                writer.WriteUInt16((ushort)x.CodeSize);
                break;

            case ActionConstantPool x:
                writer.WriteUInt16((ushort)x.Constants.Count);
                foreach (var constant in x.Constants)
                    writer.WriteNullTerminatedString(constant, encoding);
                break;

            case ActionPush x:
                foreach (var value in x.PushValues)
                    EncodePushValue(writer, value, encoding);
                break;

            case ActionDefineFunction x:
                writer.WriteNullTerminatedString(x.Name, encoding);
                writer.WriteUInt16((ushort)x.Parameters.Count);
                foreach (var parameter in x.Parameters)
                    writer.WriteNullTerminatedString(parameter, encoding);
                writer.WriteUInt16((ushort)x.CodeSize);
                break;

            case ActionDefineFunction2 x:
                writer.WriteNullTerminatedString(x.Name, encoding);
                writer.WriteUInt16((ushort)x.Parameters.Count);
                writer.WriteUInt8(x.RegisterCount);
                writer.WriteUInt16((ushort)x.Flags);
                foreach (var parameter in x.Parameters)
                {
                    writer.WriteUInt8(parameter.Register);
                    writer.WriteNullTerminatedString(parameter.Name, encoding);
                }

                writer.WriteUInt16((ushort)x.CodeSize);
                break;

            case ActionTry x:
                writer.WriteUInt8((byte)x.Flags);
                writer.WriteUInt16(x.TrySize);
                writer.WriteUInt16(x.CatchSize);
                writer.WriteUInt16(x.FinallySize);
                if (x.Flags.HasFlag(TryFlags.CatchInRegister))
                    writer.WriteUInt8(x.CatchRegister);
                else
                    writer.WriteNullTerminatedString(x.CatchVariable, encoding);
                break;

            case ActionUnknown x:
                writer.WriteMemory(x.Data);
                break;

            default:
                break;
        }
    }

    private static void EncodePushValue(MemoryWriter writer, PushValue value, Encoding encoding)
    {
        switch (value)
        {
            case PushValue.PushValueString x:
                writer.WriteUInt8(0);
                writer.WriteNullTerminatedString(x.Value, encoding);
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

    private static Action Decode(MemoryReader reader, ActionOpcode opcode, Encoding encoding)
    {
        return opcode switch
        {
            ActionOpcode.End => new ActionEnd(),
            ActionOpcode.NextFrame => new ActionNextFrame(),
            ActionOpcode.PreviousFrame => new ActionPreviousFrame(),
            ActionOpcode.Play => new ActionPlay(),
            ActionOpcode.Stop => new ActionStop(),
            ActionOpcode.ToggleQuality => new ActionToggleQuality(),
            ActionOpcode.StopSounds => new ActionStopSounds(),
            ActionOpcode.Add => new ActionAdd(),
            ActionOpcode.Subtract => new ActionSubtract(),
            ActionOpcode.Multiply => new ActionMultiply(),
            ActionOpcode.Divide => new ActionDivide(),
            ActionOpcode.Equals => new ActionEquals(),
            ActionOpcode.Less => new ActionLess(),
            ActionOpcode.And => new ActionAnd(),
            ActionOpcode.Or => new ActionOr(),
            ActionOpcode.Not => new ActionNot(),
            ActionOpcode.StringEquals => new ActionStringEquals(),
            ActionOpcode.StringLength => new ActionStringLength(),
            ActionOpcode.StringExtract => new ActionStringExtract(),
            ActionOpcode.Pop => new ActionPop(),
            ActionOpcode.ToInteger => new ActionToInteger(),
            ActionOpcode.GetVariable => new ActionGetVariable(),
            ActionOpcode.SetVariable => new ActionSetVariable(),
            ActionOpcode.SetTarget2 => new ActionSetTarget2(),
            ActionOpcode.StringAdd => new ActionStringAdd(),
            ActionOpcode.GetProperty => new ActionGetProperty(),
            ActionOpcode.SetProperty => new ActionSetProperty(),
            ActionOpcode.CloneSprite => new ActionCloneSprite(),
            ActionOpcode.RemoveSprite => new ActionRemoveSprite(),
            ActionOpcode.Trace => new ActionTrace(),
            ActionOpcode.StartDrag => new ActionStartDrag(),
            ActionOpcode.EndDrag => new ActionEndDrag(),
            ActionOpcode.StringLess => new ActionStringLess(),
            ActionOpcode.Throw => new ActionThrow(),
            ActionOpcode.CastOp => new ActionCastOp(),
            ActionOpcode.ImplementsOp => new ActionImplementsOp(),
            ActionOpcode.RandomNumber => new ActionRandomNumber(),
            ActionOpcode.MBStringLength => new ActionMBStringLength(),
            ActionOpcode.CharToAscii => new ActionCharToAscii(),
            ActionOpcode.AsciiToChar => new ActionAsciiToChar(),
            ActionOpcode.GetTime => new ActionGetTime(),
            ActionOpcode.MBStringExtract => new ActionMBStringExtract(),
            ActionOpcode.MBCharToAscii => new ActionMBCharToAscii(),
            ActionOpcode.MBAsciiToChar => new ActionMBAsciiToChar(),
            ActionOpcode.Delete => new ActionDelete(),
            ActionOpcode.Delete2 => new ActionDelete2(),
            ActionOpcode.DefineLocal => new ActionDefineLocal(),
            ActionOpcode.CallFunction => new ActionCallFunction(),
            ActionOpcode.Return => new ActionReturn(),
            ActionOpcode.Modulo => new ActionModulo(),
            ActionOpcode.NewObject => new ActionNewObject(),
            ActionOpcode.DefineLocal2 => new ActionDefineLocal2(),
            ActionOpcode.InitArray => new ActionInitArray(),
            ActionOpcode.InitObject => new ActionInitObject(),
            ActionOpcode.TypeOf => new ActionTypeOf(),
            ActionOpcode.TargetPath => new ActionTargetPath(),
            ActionOpcode.Enumerate => new ActionEnumerate(),
            ActionOpcode.Add2 => new ActionAdd2(),
            ActionOpcode.Less2 => new ActionLess2(),
            ActionOpcode.Equals2 => new ActionEquals2(),
            ActionOpcode.ToNumber => new ActionToNumber(),
            ActionOpcode.ToString => new ActionToString(),
            ActionOpcode.PushDuplicate => new ActionPushDuplicate(),
            ActionOpcode.StackSwap => new ActionStackSwap(),
            ActionOpcode.GetMember => new ActionGetMember(),
            ActionOpcode.SetMember => new ActionSetMember(),
            ActionOpcode.Increment => new ActionIncrement(),
            ActionOpcode.Decrement => new ActionDecrement(),
            ActionOpcode.CallMethod => new ActionCallMethod(),
            ActionOpcode.NewMethod => new ActionNewMethod(),
            ActionOpcode.InstanceOf => new ActionInstanceOf(),
            ActionOpcode.Enumerate2 => new ActionEnumerate2(),
            ActionOpcode.BitAnd => new ActionBitAnd(),
            ActionOpcode.BitOr => new ActionBitOr(),
            ActionOpcode.BitXor => new ActionBitXor(),
            ActionOpcode.BitLShift => new ActionBitLShift(),
            ActionOpcode.BitRShift => new ActionBitRShift(),
            ActionOpcode.BitURShift => new ActionBitURShift(),
            ActionOpcode.StrictEquals => new ActionStrictEquals(),
            ActionOpcode.Greater => new ActionGreater(),
            ActionOpcode.StringGreater => new ActionStringGreater(),
            ActionOpcode.Extends => new ActionExtends(),
            ActionOpcode.GotoFrame => ActionGotoFrame.Decode(reader),
            ActionOpcode.GetURL => ActionGetURL.Decode(reader, encoding),
            ActionOpcode.GetURL2 => ActionGetURL2.Decode(reader),
            ActionOpcode.StoreRegister => ActionStoreRegister.Decode(reader),
            ActionOpcode.ConstantPool => ActionConstantPool.Decode(reader, encoding),
            ActionOpcode.WaitForFrame => ActionWaitForFrame.Decode(reader),
            ActionOpcode.WaitForFrame2 => ActionWaitForFrame2.Decode(reader),
            ActionOpcode.SetTarget => ActionSetTarget.Decode(reader, encoding),
            ActionOpcode.GoToLabel => ActionGoToLabel.Decode(reader, encoding),
            ActionOpcode.Push => ActionPush.Decode(reader, encoding),
            ActionOpcode.Jump => ActionJump.Decode(reader),
            ActionOpcode.If => ActionIf.Decode(reader),
            ActionOpcode.Call => new ActionCall(),
            ActionOpcode.GotoFrame2 => ActionGotoFrame2.Decode(reader),
            ActionOpcode.With => ActionWith.Decode(reader),
            ActionOpcode.DefineFunction => ActionDefineFunction.Decode(reader, encoding),
            ActionOpcode.DefineFunction2 => ActionDefineFunction2.Decode(reader, encoding),
            ActionOpcode.Try => ActionTry.Decode(reader, encoding),
            _ => ActionUnknown.Decode(reader, opcode)
        };
    }
}
