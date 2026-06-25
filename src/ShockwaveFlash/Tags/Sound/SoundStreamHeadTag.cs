using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed record SoundStreamHeadTag(TagMetadata Metadata, SoundFormat StreamFormat, SoundFormat PlaybackFormat, ushort NumSamplesPerBlock, short LatencySeek) : Tag(Metadata)
{
    public static SoundStreamHeadTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var streamFormat = SoundFormat.Decode(reader);
        var playbackFormat = SoundFormat.Decode(reader);
        var numSamplesPerBlock = reader.ReadUInt16();
        var latencySeek = streamFormat.Compression is AudioCompression.Mp3 ? reader.ReadInt16() : (short)0;

        return new SoundStreamHeadTag(metadata, streamFormat, playbackFormat, numSamplesPerBlock, latencySeek);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        StreamFormat.Encode(writer);
        PlaybackFormat.Encode(writer);
        writer.WriteUInt16(NumSamplesPerBlock);

        if (StreamFormat.Compression is AudioCompression.Mp3)
            writer.WriteInt16(LatencySeek);
    }
}
