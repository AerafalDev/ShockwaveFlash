// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Shape.Gradients;

namespace ShockwaveFlash.Types.Shape;

public sealed record FillStyleFocalGradient(Gradient Gradient, float FocalPoint) : FillStyle;
