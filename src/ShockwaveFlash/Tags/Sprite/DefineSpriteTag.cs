namespace ShockwaveFlash.Tags.Sprite;

public sealed class DefineSpriteTag : Tag
{
    public ushort Id { get; set; }

    public ushort NumFrames { get; set; }

    public IReadOnlyList<Tag> Tags { get; set; }

    public DefineSpriteTag(TagMetadata metadata, ushort id, ushort numFrames, IReadOnlyList<Tag> tags) : base(metadata)
    {
        Id = id;
        NumFrames = numFrames;
        Tags = tags;
    }

    public static DefineSpriteTag Decode(MemoryReader reader, TagMetadata metadata, byte swfVersion)
    {
        var id = reader.ReadUInt16();
        var numFrames = reader.ReadUInt16();
        var tags = DecodeCollection(reader, swfVersion);

        return new DefineSpriteTag(metadata, id, numFrames, tags);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(Id);
        writer.WriteUInt16(NumFrames);
        EncodeCollection(writer, Tags, swfVersion);
    }
}
