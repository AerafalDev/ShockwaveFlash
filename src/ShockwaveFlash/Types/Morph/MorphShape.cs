// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Types.Morph;

public sealed record MorphShape(Rectangle ShapeBounds, Rectangle EdgeBounds, FillStyle[] FillStyles, LineStyle[] LineStyles, IReadOnlyList<ShapeRecord> Shapes);
