namespace ShockwaveFlash.Types.Morph;

[Flags]
public enum DefineMorphShapeFlags : byte
{
    HasScalingStrokes = 1 << 0,
    HasNonScalingStrokes = 1 << 1
}
