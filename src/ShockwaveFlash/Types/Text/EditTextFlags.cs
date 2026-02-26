// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Text;

[Flags]
public enum EditTextFlags : ushort
{
    HasFont = 1 << 0,
    HasMaxLength = 1 << 1,
    HasTextColor = 1 << 2,
    ReadOnly = 1 << 3,
    Password = 1 << 4,
    Multiline = 1 << 5,
    WordWrap = 1 << 6,
    HasText = 1 << 7,

    UseOutlines = 1 << 8,
    Html = 1 << 9,
    WasStatic = 1 << 10,
    Border = 1 << 11,
    NoSelect = 1 << 12,
    HasLayout = 1 << 13,
    AutoSize = 1 << 14,
    HasFontClass = 1 << 15
}
