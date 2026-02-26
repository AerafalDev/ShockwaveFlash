// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.DisplayList;

[Flags]
public enum ClipEventFlags : uint
{
    None = 0,
    Load = 1 << 0,
    EnterFrame = 1 << 1,
    Unload = 1 << 2,
    MouseMove = 1 << 3,
    MouseDown = 1 << 4,
    MouseUp = 1 << 5,
    KeyDown = 1 << 6,
    KeyUp = 1 << 7,
    Data = 1 << 8,
    Initialize = 1 << 9,
    Press = 1 << 10,
    Release = 1 << 11,
    ReleaseOutside = 1 << 12,
    RollOver = 1 << 13,
    RollOut = 1 << 14,
    DragOver = 1 << 15,
    DragOut = 1 << 16,
    KeyPress = 1 << 17,
    Construct = 1 << 18,
}

public static class ClipEventFlagsExtensions
{
    extension(ClipEventFlags)
    {
        public static ClipEventFlags Decode(ref SpanReader reader, byte swfVersion)
        {
            var flags = swfVersion >= 6
                ? reader.ReadUInt32()
                : reader.ReadUInt16();

            return (ClipEventFlags)flags;
        }
    }
}
