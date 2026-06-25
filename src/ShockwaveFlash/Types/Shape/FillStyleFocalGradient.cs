using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Shape;

public sealed record FillStyleFocalGradient(Gradient Gradient, float FocalPoint) : FillStyle;
