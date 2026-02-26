// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.DisplayList;

public enum BlendMode : byte
{
    Normal = 0,
    Layer = 2,
    Multiply = 3,
    Screen = 4,
    Lighten = 5,
    Darken = 6,
    Difference = 7,
    Add = 8,
    Subtract = 9,
    Invert = 10,
    Alpha = 11,
    Erase = 12,
    Overlay = 13,
    HardLight = 14
}

public static class BlendModeExtensions
{
    extension(BlendMode)
    {
        public static BlendMode Parse(byte bits)
        {
            if (bits is 1)
                bits = 0;

            return (BlendMode)bits;
        }
    }
}
