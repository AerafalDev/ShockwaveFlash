namespace ShockwaveFlash.Types.Control;

[Flags]
public enum FileAttributesFlags : uint
{
    UseDirectBlit = 1 << 6,
    UseGpu = 1 << 5,
    HasMetadata = 1 << 4,
    IsActionScript3 = 1 << 3,
    UseNetworkSandbox = 1 << 0
}
