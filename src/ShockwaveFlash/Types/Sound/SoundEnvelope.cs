namespace ShockwaveFlash.Types.Sound;

public sealed class SoundEnvelope
{
    public SoundEnvelopePoint[] Points { get; set; }

    public SoundEnvelope(SoundEnvelopePoint[] points)
    {
        Points = points;
    }
}
