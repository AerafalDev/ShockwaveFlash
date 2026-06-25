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
