// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace ShockwaveFlash.Types.Filter;

[Flags]
public enum BlurFilterFlags : byte
{
    Passes = 31 << 3
}

public static class BlurFilterFlagsExtensions
{
    extension(BlurFilterFlags)
    {
        public static BlurFilterFlags FromPasses(byte passes)
        {
            var flags = (BlurFilterFlags)(passes << 3);
            Debug.Assert((flags & BlurFilterFlags.Passes) == flags);
            return flags;
        }
    }
}
