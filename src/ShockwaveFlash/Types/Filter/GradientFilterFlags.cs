// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace ShockwaveFlash.Types.Filter;

[Flags]
public enum GradientFilterFlags : byte
{
    InnerShadow = 1 << 7,
    Knockout = 1 << 6,
    CompositeSource = 1 << 5,
    OnTop = 1 << 4,
    Passes = 15
}

public static class GradientFilterFlagsExtensions
{
    extension(GradientFilterFlags)
    {
        public static GradientFilterFlags FromPasses(byte passes)
        {
            var flags = (GradientFilterFlags)passes;
            Debug.Assert((flags & GradientFilterFlags.Passes) == flags);
            return flags;
        }
    }
}
