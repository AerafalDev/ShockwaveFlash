namespace ShockwaveFlash.Types.Shape;

public sealed class LineJoinStyleMiter : LineJoinStyle
{
    public Fixed8 MiterLimit { get; set; }

    public LineJoinStyleMiter(Fixed8 miterLimit)
    {
        MiterLimit = miterLimit;
    }
}
