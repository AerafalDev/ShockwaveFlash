namespace ShockwaveFlash.Types.Sound;

public sealed class SoundEnvelopePoint
{
    public uint Sample { get; set; }

    public float LeftVolume { get; set; }

    public float RightVolume { get; set; }

    public SoundEnvelopePoint(uint sample, float leftVolume, float rightVolume)
    {
        Sample = sample;
        LeftVolume = leftVolume;
        RightVolume = rightVolume;
    }
}
