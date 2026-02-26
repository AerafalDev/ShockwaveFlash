// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape.Gradients;

public enum GradientInterpolation
{
    Rgb = 0,
    LinearRgb = 1
}

public static class GradientInterpolationExtensions
{
    extension(GradientInterpolation)
    {
        public static GradientInterpolation Parse(byte bits)
        {
            // Per SWF19 p. 136, InterpolationMode 2 and 3 are reserved.
            // Flash treats them as normal RGB mode interpolation.
            if (bits is 2 or 3)
                bits = 0;

            return (GradientInterpolation)bits;
        }
    }
}
