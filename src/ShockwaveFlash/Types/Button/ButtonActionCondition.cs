// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Button;

[Flags]
public enum ButtonActionCondition : ushort
{
    IdleToOverUp = 1 << 0,
    OverUpToIdle = 1 << 1,
    OverUpToOverDown = 1 << 2,
    OverDownToOverUp = 1 << 3,
    OverDownToOutDown = 1 << 4,
    OutDownToOverDown = 1 << 5,
    OutDownToIdle = 1 << 6,
    IdleToOverDown = 1 << 7,
    OverDownToIdle = 1 << 8,
    KeyPress = 127 << 9
}

public static class ButtonActionConditionExtensions
{
    extension(ButtonActionCondition)
    {
        public static ButtonActionCondition FromKeyCode(byte keyCode)
        {
            return (ButtonActionCondition)(keyCode << 9);
        }
    }

    extension(ButtonActionCondition self)
    {
        public bool Matches(ButtonActionCondition testCondition)
        {
            var selfKeyPress = (byte)(self & ButtonActionCondition.KeyPress);
            var testKeyPress = (byte)(testCondition & ButtonActionCondition.KeyPress);
            var selfWithoutKey = self & ~ButtonActionCondition.KeyPress;
            var testWithoutKey = testCondition & ~ButtonActionCondition.KeyPress;

            return selfWithoutKey.HasFlag(testWithoutKey) && (testKeyPress is 0 || testKeyPress == selfKeyPress);
        }
    }
}
