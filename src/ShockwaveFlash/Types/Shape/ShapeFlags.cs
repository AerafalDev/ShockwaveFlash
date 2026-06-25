namespace ShockwaveFlash.Types.Shape;

[Flags]
public enum ShapeFlags : byte
{
    HasScalingStrokes = 1 << 0,
    HasNonScalingStrokes = 1 << 1,
    NonZeroWindingRule = 1 << 2
}
