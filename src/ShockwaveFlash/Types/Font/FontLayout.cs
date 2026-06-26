namespace ShockwaveFlash.Types.Font;

public sealed class FontLayout
{
    public ushort Ascent { get; set; }

    public ushort Descent { get; set; }

    public short Leading { get; set; }

    public FontKerning[] Kerning { get; set; }

    public FontLayout(ushort ascent, ushort descent, short leading, FontKerning[] kerning)
    {
        Ascent = ascent;
        Descent = descent;
        Leading = leading;
        Kerning = kerning;
    }
}
