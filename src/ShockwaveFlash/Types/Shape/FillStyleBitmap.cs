namespace ShockwaveFlash.Types.Shape;

public sealed record FillStyleBitmap(ushort Id, Matrix Matrix, bool IsSmoothed, bool IsRepeating) : FillStyle;
