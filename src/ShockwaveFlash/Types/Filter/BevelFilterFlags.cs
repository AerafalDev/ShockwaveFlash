using System.Diagnostics;

namespace ShockwaveFlash.Types.Filter;

[Flags]
public enum BevelFilterFlags : byte
{
    InnerShadow = 1 << 7,
    Knockout = 1 << 6,
    CompositeSource = 1 << 5,
    OnTop = 1 << 4,
    Passes = 0b1111
}

public static class BevelFilterFlagsExtensions
{
    extension(BevelFilterFlags)
    {
        public static BevelFilterFlags FromPasses(byte passes)
        {
            var flags = (BevelFilterFlags)passes;
            Debug.Assert((flags & BevelFilterFlags.Passes) == flags);
            return flags;
        }
    }
}
