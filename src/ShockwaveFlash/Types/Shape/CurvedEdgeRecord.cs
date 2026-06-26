namespace ShockwaveFlash.Types.Shape;

public sealed class CurvedEdgeRecord : ShapeRecord
{
    public Point ControlDelta { get; set; }

    public Point AnchorDelta { get; set; }

    public CurvedEdgeRecord(Point controlDelta, Point anchorDelta)
    {
        ControlDelta = controlDelta;
        AnchorDelta = anchorDelta;
    }
}
