using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Shape;

public sealed record FillStyleFocalGradient(Gradient Gradient, Fixed8 FocalPoint) : FillStyle;
