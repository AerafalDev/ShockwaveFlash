// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape.Gradients;

public enum GradientSpread : byte
{
    Pad = 0,
    Reflect = 1,
    Repeat = 2
}

public static class GradientSpreadExtensions
{
    extension(GradientSpread)
    {
        public static GradientSpread Parse(byte bits)
        {
            // Per SWF19 p. 136, SpreadMode 3 is reserved.
            // Flash treats it as pad mode.
            if (bits is 3)
                bits = 0;

            return (GradientSpread)bits;
        }
    }
}
