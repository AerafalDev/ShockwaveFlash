using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Shape;

public sealed class FillStyleFocalGradient : FillStyle
{
    public Gradient Gradient { get; set; }

    public Fixed8 FocalPoint { get; set; }

    public FillStyleFocalGradient(Gradient gradient, Fixed8 focalPoint)
    {
        Gradient = gradient;
        FocalPoint = focalPoint;
    }
}
