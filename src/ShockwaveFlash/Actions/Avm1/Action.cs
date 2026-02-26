// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using ShockwaveFlash.Actions.Avm1.Special;
using ShockwaveFlash.Actions.Avm1.Swf1;
using ShockwaveFlash.Actions.Avm1.Swf2;
using ShockwaveFlash.Actions.Avm1.Swf3;
using ShockwaveFlash.Actions.Avm1.Swf4;
using ShockwaveFlash.Actions.Avm1.Swf5;
using ShockwaveFlash.Actions.Avm1.Swf6;
using ShockwaveFlash.Actions.Avm1.Swf7;

namespace ShockwaveFlash.Actions.Avm1;

public abstract record Action(ActionOpcode Opcode)
{
    public static IReadOnlyList<Action> DecodeCollection(in ReadOnlySpan<byte> buffer, byte swfVersion)
    {
        var encoding = swfVersion >= 6 ? Encoding.UTF8 : Encoding.GetEncoding("iso-8859-1");
        var actions = new List<Action>(capacity: 64);
        var reader = new SpanReader(buffer);

        while (reader.Remaining > 0)
        {
            var opcodeRaw = reader.ReadUInt8();
            var opcode = (ActionOpcode)opcodeRaw;

            var payloadLength = 0;

            if (opcodeRaw >= 128)
                payloadLength = reader.ReadUInt16();

            var actionReader = new SpanReader(reader.ReadSpan(payloadLength));
            var action = Decode(ref actionReader, opcode, encoding);

            if (actionReader.Remaining > 0)
                throw new Exception($"Action length mismatch. Expected {payloadLength} bytes, got {actionReader.Position}.");

            actions.Add(action);

            if (action is ActionEnd)
                break;
        }

        return actions;
    }

    private static Action Decode(ref SpanReader reader, ActionOpcode opcode, Encoding encoding)
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
            ActionOpcode.GotoFrame => ActionGotoFrame.Decode(ref reader),
            ActionOpcode.GetURL => ActionGetURL.Decode(ref reader, encoding),
            ActionOpcode.GetURL2 => ActionGetURL2.Decode(ref reader),
            ActionOpcode.StoreRegister => ActionStoreRegister.Decode(ref reader),
            ActionOpcode.ConstantPool => ActionConstantPool.Decode(ref reader, encoding),
            ActionOpcode.WaitForFrame => ActionWaitForFrame.Decode(ref reader),
            ActionOpcode.WaitForFrame2 => ActionWaitForFrame2.Decode(ref reader),
            ActionOpcode.SetTarget => ActionSetTarget.Decode(ref reader, encoding),
            ActionOpcode.GoToLabel => ActionGoToLabel.Decode(ref reader, encoding),
            ActionOpcode.Push => ActionPush.Decode(ref reader, encoding),
            ActionOpcode.Jump => ActionJump.Decode(ref reader),
            ActionOpcode.If => ActionIf.Decode(ref reader),
            ActionOpcode.Call => new ActionCall(),
            ActionOpcode.GotoFrame2 => ActionGotoFrame2.Decode(ref reader),
            ActionOpcode.With => ActionWith.Decode(ref reader),
            ActionOpcode.DefineFunction => ActionDefineFunction.Decode(ref reader, encoding),
            ActionOpcode.DefineFunction2 => ActionDefineFunction2.Decode(ref reader, encoding),
            ActionOpcode.Try => ActionTry.Decode(ref reader, encoding),
            _ => ActionUnknown.Decode(ref reader, opcode)
        };
    }
}
