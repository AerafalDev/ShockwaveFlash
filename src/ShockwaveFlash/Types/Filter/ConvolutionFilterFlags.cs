namespace ShockwaveFlash.Types.Filter;

[Flags]
public enum ConvolutionFilterFlags : byte
{
    Clamp = 1 << 1,
    PreserveAlpha = 1 << 0
}
