using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Shape;

public sealed class FillStyleRadialGradient : FillStyle
{
    public Gradient Gradient { get; set; }

    public FillStyleRadialGradient(Gradient gradient)
    {
        Gradient = gradient;
    }
}
