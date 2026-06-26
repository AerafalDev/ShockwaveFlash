namespace ShockwaveFlash.Types.Shape;

public sealed class FillStyleSolid : FillStyle
{
    public Color Color { get; set; }

    public FillStyleSolid(Color color)
    {
        Color = color;
    }
}
