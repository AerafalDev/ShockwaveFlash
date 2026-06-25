namespace ShockwaveFlash.Tags.Control;

public sealed record ScriptLimitsTag(TagMetadata Metadata, ushort MaxRecursionDepth, TimeSpan Timeout) : Tag(Metadata)
{
    public static ScriptLimitsTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new ScriptLimitsTag(metadata, reader.ReadUInt16(), TimeSpan.FromSeconds(reader.ReadUInt16()));
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16(MaxRecursionDepth);
        writer.WriteUInt16((ushort)Timeout.TotalSeconds);
    }
}
