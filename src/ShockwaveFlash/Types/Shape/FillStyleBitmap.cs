namespace ShockwaveFlash.Types.Shape;

public sealed class FillStyleBitmap : FillStyle
{
    public ushort Id { get; set; }

    public Matrix Matrix { get; set; }

    public bool IsSmoothed { get; set; }

    public bool IsRepeating { get; set; }

    public FillStyleBitmap(ushort id, Matrix matrix, bool isSmoothed, bool isRepeating)
    {
        Id = id;
        Matrix = matrix;
        IsSmoothed = isSmoothed;
        IsRepeating = isRepeating;
    }
}
