namespace ShockwaveFlash.Types.Sound;

public sealed record SoundInfo(SoundEvent Event, uint? InSample, uint? OutSample, ushort NumLoops, SoundEnvelope? Envelope)
{
    public static SoundInfo Decode(MemoryReader reader)
    {
        var flags = reader.ReadUInt8();
        var sEvent = SoundEvent.Parse((byte)((flags >> 4) & 3));
        uint? inSample = (flags & 1) is not 0 ? reader.ReadUInt32() : null;
        uint? outSample = (flags & 2) is not 0 ? reader.ReadUInt32() : null;
        var numLoops = (flags & 4) is not 0 ? reader.ReadUInt16() : (ushort)1;
        SoundEnvelope? envelope = null;

        if ((flags & 8) is not 0)
        {
            var numPoints = reader.ReadUInt8();
            var points = new SoundEnvelopePoint[numPoints];

            for (var i = 0; i < numPoints; i++)
            {
                var sample = reader.ReadUInt32();
                var leftVolume = reader.ReadUInt16() / 32768f;
                var rightVolume = reader.ReadUInt16() / 32768f;
                points[i] = new SoundEnvelopePoint(sample, leftVolume, rightVolume);
            }

            envelope = new SoundEnvelope(points);
        }

        return new SoundInfo(sEvent, inSample, outSample, numLoops, envelope);
    }

    public void Encode(MemoryWriter writer)
    {
        var hasInSample = InSample is not null;
        var hasOutSample = OutSample is not null;
        var hasLoops = NumLoops is not 1;
        var hasEnvelope = Envelope is not null;

        var flags = (byte)((((byte)Event & 3) << 4)
            | (hasEnvelope ? 8 : 0)
            | (hasLoops ? 4 : 0)
            | (hasOutSample ? 2 : 0)
            | (hasInSample ? 1 : 0));

        writer.WriteUInt8(flags);

        if (InSample is { } inSample)
            writer.WriteUInt32(inSample);

        if (OutSample is { } outSample)
            writer.WriteUInt32(outSample);

        if (hasLoops)
            writer.WriteUInt16(NumLoops);

        if (Envelope is { } envelope)
        {
            writer.WriteUInt8((byte)envelope.Points.Length);

            foreach (var point in envelope.Points)
            {
                writer.WriteUInt32(point.Sample);
                writer.WriteUInt16((ushort)(point.LeftVolume * 32768f));
                writer.WriteUInt16((ushort)(point.RightVolume * 32768f));
            }
        }
    }
}
