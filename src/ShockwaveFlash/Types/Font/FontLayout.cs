// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Font;

public sealed record FontLayout(ushort Ascent, ushort Descent, short Leading, FontKerning[] Kerning);
