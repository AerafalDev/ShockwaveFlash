namespace ShockwaveFlash.Types.Font;

public sealed record FontLayout(ushort Ascent, ushort Descent, short Leading, FontKerning[] Kerning);
