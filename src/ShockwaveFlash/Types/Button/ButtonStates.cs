namespace ShockwaveFlash.Types.Button;

[Flags]
public enum ButtonStates : byte
{
    Up = 1 << 0,
    Over = 1 << 1,
    Down = 1 << 2,
    HitTest = 1 << 3
}
