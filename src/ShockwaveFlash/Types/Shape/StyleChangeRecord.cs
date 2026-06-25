namespace ShockwaveFlash.Types.Shape;

public sealed record StyleChangeRecord(Point? MoveTo, uint? FillStyle0, uint? FillStyle1, uint? LineStyle, ShapeStyles? NewStyles) : ShapeRecord;
