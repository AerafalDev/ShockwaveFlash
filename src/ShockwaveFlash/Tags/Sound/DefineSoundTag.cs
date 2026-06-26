using ShockwaveFlash.Types.Sound;

namespace ShockwaveFlash.Tags.Sound;

public sealed class DefineSoundTag : Tag
{
    public ushort Id { get; set; }

    public SoundFormat Format { get; set; }

    public uint NumSamples { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public DefineSoundTag(TagMetadata metadata, ushort id, SoundFormat format, uint numSamples, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Id = id;
        Format = format;
        NumSamples = numSamples;
        Data = data;
    }

    public static DefineSoundTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var format = SoundFormat.Decode(reader);
        var numSamples = reader.ReadUInt32();
        var data = reader.ReadMemoryToEnd();

        return new DefineSoundTag(metadata, id, format, numSamples, data);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        Format.Encode(writer);
        writer.WriteUInt32(NumSamples);
        writer.WriteMemory(Data);
    }
}
