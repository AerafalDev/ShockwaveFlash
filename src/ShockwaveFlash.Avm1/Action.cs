using ShockwaveFlash.Avm1.Special;
using ShockwaveFlash.Avm1.Swf1;
using ShockwaveFlash.Avm1.Swf2;
using ShockwaveFlash.Avm1.Swf3;
using ShockwaveFlash.Avm1.Swf4;
using ShockwaveFlash.Avm1.Swf5;
using ShockwaveFlash.Avm1.Swf6;
using ShockwaveFlash.Avm1.Swf7;
using ShockwaveFlash.Exceptions;

namespace ShockwaveFlash.Avm1;

public abstract record Action(ActionOpcode Opcode)
{
    public static IReadOnlyList<Action> DecodeCollection(ReadOnlyMemory<byte> buffer, byte swfVersion)
    {
        var context = new Avm1Context(swfVersion);
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
            var action = Decode(actionReader, reader, opcode, context);

            if (actionReader.Remaining > 0)
                throw new SwfFormatException($"AVM1 action {opcode} declared {payloadLength} bytes but consumed {actionReader.Position}.");

            actions.Add(action);

            if (action is ActionEnd)
                break;
        }

        return actions;
    }

    public static ReadOnlyMemory<byte> EncodeCollection(IReadOnlyList<Action> actions, byte swfVersion)
    {
        var context = new Avm1Context(swfVersion);
        var writer = new MemoryWriter();
        var body = new MemoryWriter();

        foreach (var action in actions)
        {
            var opcode = (byte)action.Opcode;
            writer.WriteUInt8(opcode);

            if (opcode < 128)
                continue;

            body.Reset();
            action.Encode(body, context);

            if (body.Position > ushort.MaxValue)
                throw new SwfFormatException($"AVM1 action {action.Opcode} body of {body.Position} bytes exceeds the 65535-byte action limit.");

            writer.WriteUInt16((ushort)body.Position);
            writer.WriteMemory(body.WrittenMemory);
            action.EncodeTrailer(writer);
        }

        return writer.WrittenMemory;
    }

    public virtual void Encode(MemoryWriter writer, Avm1Context context)
    {
    }

    public virtual void EncodeTrailer(MemoryWriter writer)
    {
    }

    private static Action Decode(MemoryReader reader, MemoryReader outer, ActionOpcode opcode, Avm1Context context)
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
            ActionOpcode.GetURL => ActionGetURL.Decode(reader, context.Encoding),
            ActionOpcode.GetURL2 => ActionGetURL2.Decode(reader),
            ActionOpcode.StoreRegister => ActionStoreRegister.Decode(reader),
            ActionOpcode.ConstantPool => ActionConstantPool.Decode(reader, context.Encoding),
            ActionOpcode.WaitForFrame => ActionWaitForFrame.Decode(reader),
            ActionOpcode.WaitForFrame2 => ActionWaitForFrame2.Decode(reader),
            ActionOpcode.SetTarget => ActionSetTarget.Decode(reader, context.Encoding),
            ActionOpcode.GoToLabel => ActionGoToLabel.Decode(reader, context.Encoding),
            ActionOpcode.Push => ActionPush.Decode(reader, context.Encoding),
            ActionOpcode.Jump => ActionJump.Decode(reader),
            ActionOpcode.If => ActionIf.Decode(reader),
            ActionOpcode.Call => new ActionCall(),
            ActionOpcode.GotoFrame2 => ActionGotoFrame2.Decode(reader),
            ActionOpcode.With => ActionWith.Decode(reader, outer),
            ActionOpcode.DefineFunction => ActionDefineFunction.Decode(reader, outer, context),
            ActionOpcode.DefineFunction2 => ActionDefineFunction2.Decode(reader, outer, context),
            ActionOpcode.Try => ActionTry.Decode(reader, outer, context),
            _ => ActionUnknown.Decode(reader, opcode)
        };
    }
}
