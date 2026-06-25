namespace ShockwaveFlash.Types.Font;

[Flags]
public enum FontInfoFlags : byte
{
    HasWideCodes = 1 << 0,
    IsBold = 1 << 1,
    IsItalic = 1 << 2,
    IsShiftJis = 1 << 3,
    IsAnsi = 1 << 4,
    IsSmallText = 1 << 5
}
