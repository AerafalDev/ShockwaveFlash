// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Shape;

public sealed record StyleChangeRecord(Point? MoveTo, uint? FillStyle0, uint? FillStyle1, uint? LineStyle, ShapeStyles? NewStyles) : ShapeRecord;
