// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace ShockwaveFlash.Types.Filter;

[Flags]
public enum GlowFilterFlags : byte
{
    InnerGlow = 1 << 7,
    Knockout = 1 << 6,
    CompositeSource = 1 << 5,
    Passes = 31
}

public static class GlowFilterFlagsExtensions
{
    extension(GlowFilterFlags)
    {
        public static GlowFilterFlags FromPasses(byte passes)
        {
            var flags = (GlowFilterFlags)passes;
            Debug.Assert((flags & GlowFilterFlags.Passes) == flags);
            return flags;
        }
    }

    extension(GlowFilterFlags self)
    {
        public GlowFilterFlags Set(GlowFilterFlags flags, bool condition)
        {
            return condition ? self | flags : self & ~flags;
        }
    }
}
