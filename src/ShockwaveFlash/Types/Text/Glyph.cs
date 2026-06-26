namespace ShockwaveFlash.Types.Text;

public sealed class Glyph
{
    public uint Index { get; set; }

    public int Advance { get; set; }

    public Glyph(uint index, int advance)
    {
        Index = index;
        Advance = advance;
    }
}
