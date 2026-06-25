namespace ShockwaveFlash.Tags.Sprite;

public sealed record DefineSpriteTag(TagMetadata Metadata, ushort Id, ushort NumFrames, IReadOnlyList<Tag> Tags) : Tag(Metadata)
{
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
