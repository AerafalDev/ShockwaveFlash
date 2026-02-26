// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Types.Sound;

public sealed record SoundInfo(SoundEvent Event, uint? InSample, uint? OutSample, ushort NumLoops, SoundEnvelope? Envelope)
{
    public static SoundInfo Decode(ref SpanReader reader)
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
}
