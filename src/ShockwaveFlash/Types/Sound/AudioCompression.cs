// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Sound;

public enum AudioCompression : byte
{
    UncompressedUnknownEndian = 0,
    Adpcm = 1,
    Mp3 = 2,
    Uncompressed = 3,
    Nellymoser16Khz = 4,
    Nellymoser8Khz = 5,
    Nellymoser = 6,
    Aac = 10,
    Speex = 11
}
