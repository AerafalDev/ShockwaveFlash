// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Actions.Types;

public abstract record PushValue
{
    public sealed record PushValueUndefined : PushValue;

    public sealed record PushValueNull : PushValue;

    public sealed record PushValueBoolean(bool Value) : PushValue;

    public sealed record PushValueInteger(int Value) : PushValue;

    public sealed record PushValueFloat(float Value) : PushValue;

    public sealed record PushValueDouble(double Value) : PushValue;

    public sealed record PushValueString(string Value) : PushValue;

    public sealed record PushValueRegister(byte RegisterIndex) : PushValue;

    public sealed record PushValueConstant8(byte ConstantIndex) : PushValue;

    public sealed record PushValueConstant16(ushort ConstantIndex) : PushValue;

    public static PushValue Undefined()
    {
        return new PushValueUndefined();
    }

    public static PushValue Null()
    {
        return new PushValueNull();
    }

    public static PushValue Boolean(bool value)
    {
        return new PushValueBoolean(value);
    }

    public static PushValue Integer(int value)
    {
        return new PushValueInteger(value);
    }

    public static PushValue Float(float value)
    {
        return new PushValueFloat(value);
    }

    public static PushValue Double(double value)
    {
        return new PushValueDouble(value);
    }

    public static PushValue String(string value)
    {
        return new PushValueString(value);
    }

    public static PushValue Register(byte registerIndex)
    {
        return new PushValueRegister(registerIndex);
    }

    public static PushValue Constant8(byte constantIndex)
    {
        return new PushValueConstant8(constantIndex);
    }

    public static PushValue Constant16(ushort constantIndex)
    {
        return new PushValueConstant16(constantIndex);
    }
}
