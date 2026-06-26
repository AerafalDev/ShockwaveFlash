using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Metadata;

public sealed class FileAttributesTag : Tag
{
    public FileAttributesFlags Flags { get; set; }

    public bool UseDirectBlit =>
        Flags.HasFlag(FileAttributesFlags.UseDirectBlit);

    public bool UseGpu =>
        Flags.HasFlag(FileAttributesFlags.UseGpu);

    public bool HasMetadata =>
        Flags.HasFlag(FileAttributesFlags.HasMetadata);

    public bool IsActionScript3 =>
        Flags.HasFlag(FileAttributesFlags.IsActionScript3);

    public bool UseNetworkSandbox =>
        Flags.HasFlag(FileAttributesFlags.UseNetworkSandbox);

    public FileAttributesTag(TagMetadata metadata, FileAttributesFlags flags) : base(metadata)
    {
        Flags = flags;
    }

    public static FileAttributesTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new FileAttributesTag(metadata, (FileAttributesFlags)reader.ReadUInt32());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt32((uint)Flags);
    }
}
