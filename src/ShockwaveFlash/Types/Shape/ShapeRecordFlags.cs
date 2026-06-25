namespace ShockwaveFlash.Types.Shape;

[Flags]
public enum ShapeRecordFlags : byte
{
    None = 0,
    MoveTo = 1 << 0,
    FillStyle0 = 1 << 1,
    FillStyle1 = 1 << 2,
    LineStyle = 1 << 3,
    NewStyles = 1 << 4
}
