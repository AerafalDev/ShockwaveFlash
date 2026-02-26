// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Shape;

namespace ShockwaveFlash.Tags;

public sealed record DefineShape2Tag(TagMetadata Metadata, ushort ShapeId, Rectangle ShapeBounds, ShapeStyles Styles, IReadOnlyList<ShapeRecord> Shapes) : DefineShapeTag(Metadata, ShapeId, ShapeBounds, Styles, Shapes);

