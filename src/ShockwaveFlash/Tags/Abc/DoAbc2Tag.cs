using ShockwaveFlash.Types.Abc;

namespace ShockwaveFlash.Tags.Abc;

public sealed class DoAbc2Tag : Tag
{
    public DoAbc2Flags Flags { get; set; }

    public string Name { get; set; }

    public ReadOnlyMemory<byte> Data { get; set; }

    public DoAbc2Tag(TagMetadata metadata, DoAbc2Flags flags, string name, ReadOnlyMemory<byte> data) : base(metadata)
    {
        Flags = flags;
        Name = name;
        Data = data;
    }

    public static DoAbc2Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var flags = (DoAbc2Flags)reader.ReadUInt32();
        var name = reader.ReadNullTerminatedString();
        var data = reader.ReadMemoryToEnd();

        return new DoAbc2Tag(metadata, flags, name, data);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt32((uint)Flags);
        writer.WriteNullTerminatedString(Name);
        writer.WriteMemory(Data);
    }
}
