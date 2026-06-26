namespace ShockwaveFlash.Tags.Control;

public sealed class ScriptLimitsTag : Tag
{
    public ushort MaxRecursionDepth { get; set; }

    public TimeSpan Timeout { get; set; }

    public ScriptLimitsTag(TagMetadata metadata, ushort maxRecursionDepth, TimeSpan timeout) : base(metadata)
    {
        MaxRecursionDepth = maxRecursionDepth;
        Timeout = timeout;
    }

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
