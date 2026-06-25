namespace ShockwaveFlash.Types.Font;

[Flags]
public enum FontFlags : byte
{
    IsBold = 1 << 0,
    IsItalic = 1 << 1,
    HasWideCodes = 1 << 2,
    HasWideOffsets = 1 << 3,
    IsAnsi = 1 << 4,
    IsSmallText = 1 << 5,
    IsShiftJis = 1 << 6,
    HasLayout = 1 << 7
}
