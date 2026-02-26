// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Sound;

public enum SoundEvent : byte
{
    Event = 0,
    Start = 1,
    Stop = 2
}

public static class SoundEventExtensions
{
    extension(SoundEvent)
    {
        public static SoundEvent Parse(byte bits)
        {
            if (bits is 3)
                bits = 2;

            return (SoundEvent)bits;
        }
    }
}
