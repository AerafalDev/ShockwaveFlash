using ShockwaveFlash.IO.Binary;
using ShockwaveFlash.Tags;
using ShockwaveFlash.Tags.Button;
using ShockwaveFlash.Tags.Sound;
using ShockwaveFlash.Tags.Video;
using ShockwaveFlash.Types;
using ShockwaveFlash.Types.Button;
using ShockwaveFlash.Types.DisplayList;
using ShockwaveFlash.Types.Filter;
using ShockwaveFlash.Types.Sound;
using ShockwaveFlash.Types.Video;
using Shouldly;

namespace ShockwaveFlash.Tests;

public sealed class TagRoundTripTests
{
    [Fact]
    public void DefineSound_round_trips()
    {
        var tag = new DefineSoundTag(
            Metadata(TagCode.DefineSound),
            id: 3,
            format: new SoundFormat(AudioCompression.Mp3, 44100, isStereo: true, is16Bit: true),
            numSamples: 8192,
            data: new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });

        ShouldRoundtrip(tag, DefineSoundTag.Decode);
    }

    [Fact]
    public void SoundStreamHead_round_trips_with_mp3_latency_seek()
    {
        var tag = new SoundStreamHeadTag(
            Metadata(TagCode.SoundStreamHead),
            streamFormat: new SoundFormat(AudioCompression.Mp3, 22050, isStereo: false, is16Bit: true),
            playbackFormat: new SoundFormat(AudioCompression.Uncompressed, 44100, isStereo: true, is16Bit: true),
            numSamplesPerBlock: 1152,
            latencySeek: 576);

        ShouldRoundtrip(tag, SoundStreamHeadTag.Decode);
    }

    [Fact]
    public void StartSound_round_trips_with_in_out_and_loops()
    {
        var tag = new StartSoundTag(
            Metadata(TagCode.StartSound),
            id: 3,
            soundInfo: new SoundInfo(SoundEvent.Start, inSample: 100, outSample: 5000, numLoops: 3, envelope: null));

        ShouldRoundtrip(tag, StartSoundTag.Decode);
    }

    [Fact]
    public void DefineVideoStream_round_trips()
    {
        var tag = new DefineVideoStreamTag(
            Metadata(TagCode.DefineVideoStream),
            id: 7,
            numFrames: 30,
            width: 320,
            height: 240,
            isSmoothed: true,
            deblocking: VideoDeblocking.Level2,
            codec: VideoCodec.Vp6);

        ShouldRoundtrip(tag, DefineVideoStreamTag.Decode);
    }

    [Fact]
    public void VideoFrame_round_trips()
    {
        var tag = new VideoFrameTag(
            Metadata(TagCode.VideoFrame),
            id: 7,
            frameNum: 12,
            data: new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44 });

        ShouldRoundtrip(tag, VideoFrameTag.Decode);
    }

    [Fact]
    public void DefineButton_v1_round_trips()
    {
        var record = new ButtonRecord(
            ButtonStates.Up | ButtonStates.Over | ButtonStates.Down | ButtonStates.HitTest,
            id: 5,
            depth: 1,
            Matrix.Identity,
            ColorTransform.Identity,
            [],
            BlendMode.Normal);

        var tag = new DefineButtonTag(
            Metadata(TagCode.DefineButton),
            id: 10,
            records: [record],
            actions: [new ButtonAction(ButtonActionCondition.OverDownToOverUp, new byte[] { 0x06, 0x00 })]);

        ShouldRoundtrip(tag, DefineButtonTag.Decode);
    }

    [Fact]
    public void BlurFilter_round_trips()
    {
        Filter filter = new BlurFilter(
            new FixedPoint2(new Fixed16(5 << 16), new Fixed16(3 << 16)),
            (BlurFilterFlags)(2 << 3));

        ShouldRoundtripFilter(filter);
    }

    [Fact]
    public void GlowFilter_round_trips()
    {
        Filter filter = new GlowFilter(
            new Color(255, 128, 64, 200),
            new FixedPoint2(new Fixed16(4 << 16), new Fixed16(4 << 16)),
            new Fixed8(256),
            GlowFilterFlags.CompositeSource | (GlowFilterFlags)2);

        ShouldRoundtripFilter(filter);
    }

    [Fact]
    public void DropShadowFilter_round_trips()
    {
        Filter filter = new DropShadowFilter(
            new Color(10, 20, 30, 255),
            new FixedPoint2(new Fixed16(6 << 16), new Fixed16(6 << 16)),
            new Fixed16(45 << 16),
            new Fixed16(8 << 16),
            new Fixed8(128),
            DropShadowFilterFlags.InnerShadow | (DropShadowFilterFlags)3);

        ShouldRoundtripFilter(filter);
    }

    private static TagMetadata Metadata(TagCode code) => new(code, 0, 0);

    private static void ShouldRoundtrip<T>(T tag, Func<MemoryReader, TagMetadata, T> decode, byte swfVersion = 6) where T : Tag
    {
        var writer = new MemoryWriter();
        tag.Encode(writer, swfVersion);

        var body = writer.WrittenMemory;
        tag.Metadata = new TagMetadata(tag.Metadata.Code, 0, body.Length);

        var clone = decode(new MemoryReader(body), tag.Metadata);

        DeepEquality.Equals(tag, clone, typeof(T).Name, out var where).ShouldBeTrue(where);
    }

    private static void ShouldRoundtripFilter(Filter filter)
    {
        var writer = new MemoryWriter();
        filter.Encode(writer);

        var clone = Filter.Decode(new MemoryReader(writer.WrittenMemory));

        DeepEquality.Equals(filter, clone, filter.GetType().Name, out var where).ShouldBeTrue(where);
    }
}
