// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.DisplayList;

[Flags]
public enum PlaceObjectFlags : ushort
{
    Move = 1 << 0,
    HasCharacter = 1 << 1,
    HasMatrix = 1 << 2,
    HasColorTransform = 1 << 3,
    HasRatio = 1 << 4,
    HasName = 1 << 5,
    HasClipDepth = 1 << 6,
    HasClipActions = 1 << 7,

    HasFilterList = 1 << 8,
    HasBlendMode = 1 << 9,
    HasCacheAsBitmap = 1 << 10,
    HasClassName = 1 << 11,
    HasImage = 1 << 12,
    HasVisible = 1 << 13,
    OpaqueBackground = 1 << 14
}
