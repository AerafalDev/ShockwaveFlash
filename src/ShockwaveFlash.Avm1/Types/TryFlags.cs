namespace ShockwaveFlash.Avm1.Types;

[Flags]
public enum TryFlags : byte
{
    CatchBlock = 1 << 0,
    FinallyBlock = 1 << 1,
    CatchInRegister = 1 << 2
}
