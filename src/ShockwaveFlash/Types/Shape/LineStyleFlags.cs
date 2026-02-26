// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

[Flags]
public enum LineStyleFlags : ushort
{
    PixelHinting = 1 << 0,
    NoVScale = 1 << 1,
    NoHScale = 1 << 2,
    HasFill = 1 << 3,
    JoinStyle = 0b11 << 4,
    StartCapStyle = 0b11 << 6,

    EndCapStyle = 0b11 << 8,
    NoClose = 1 << 10,

    Round = 0,
    Bevel = 0b01 << 4,
    Miter = 0b10 << 4,
}

public static class LineStyleFlagsExtensions
{
    extension(LineStyleFlags self)
    {
        public LineStyleFlags Add(LineStyleFlags other)
        {
            return self | other;
        }

        public LineStyleFlags Remove(LineStyleFlags other)
        {
            return self & ~other;
        }

        public LineStyleFlags Set(LineStyleFlags other, bool condition)
        {
            return condition ? self.Add(other) : self.Remove(other);
        }
    }
}
