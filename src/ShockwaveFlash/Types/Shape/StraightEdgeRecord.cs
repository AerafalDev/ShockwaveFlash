namespace ShockwaveFlash.Types.Shape;

public sealed class StraightEdgeRecord : ShapeRecord
{
    public Point Delta { get; set; }

    public StraightEdgeRecord(Point delta)
    {
        Delta = delta;
    }
}
