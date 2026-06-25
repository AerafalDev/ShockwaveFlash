namespace ShockwaveFlash.Types.Shape;

public sealed record CurvedEdgeRecord(Point ControlDelta, Point AnchorDelta) : ShapeRecord;
