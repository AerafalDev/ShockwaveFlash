using System.Diagnostics;

namespace ShockwaveFlash.Types.Filter;

[Flags]
public enum DropShadowFilterFlags : byte
{
    InnerShadow = 1 << 7,
    Knockout = 1 << 6,
    CompositeSource = 1 << 5,
    Passes = 31
}

public static class DropShadowFilterFlagsExtensions
{
    extension(DropShadowFilterFlags)
    {
        public static DropShadowFilterFlags FromPasses(byte passes)
        {
            var flags = (DropShadowFilterFlags)passes;
            Debug.Assert((flags & DropShadowFilterFlags.Passes) == flags);
            return flags;
        }
    }
}
