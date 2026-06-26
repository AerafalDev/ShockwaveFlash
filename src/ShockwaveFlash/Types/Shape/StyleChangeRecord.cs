namespace ShockwaveFlash.Types.Shape;

public sealed class StyleChangeRecord : ShapeRecord
{
    public Point? MoveTo { get; set; }

    public uint? FillStyle0 { get; set; }

    public uint? FillStyle1 { get; set; }

    public uint? LineStyle { get; set; }

    public ShapeStyles? NewStyles { get; set; }

    public StyleChangeRecord(Point? moveTo, uint? fillStyle0, uint? fillStyle1, uint? lineStyle, ShapeStyles? newStyles)
    {
        MoveTo = moveTo;
        FillStyle0 = fillStyle0;
        FillStyle1 = fillStyle1;
        LineStyle = lineStyle;
        NewStyles = newStyles;
    }
}
