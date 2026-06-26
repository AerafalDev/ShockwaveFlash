using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Shape;

public sealed class FillStyleLinearGradient : FillStyle
{
    public Gradient Gradient { get; set; }

    public FillStyleLinearGradient(Gradient gradient)
    {
        Gradient = gradient;
    }
}
