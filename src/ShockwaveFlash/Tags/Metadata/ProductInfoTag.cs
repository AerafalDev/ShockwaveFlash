using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Metadata;

public sealed record ProductInfoTag(TagMetadata Metadata, FlashProduct ProductId, FlashEdition Edition, byte MajorVersion, byte MinorVersion, uint BuildLow, uint BuildHigh, DateTime CompilationDate) : Tag(Metadata)
{
    public static ProductInfoTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var productId = (FlashProduct)reader.ReadUInt32();
        var edition = (FlashEdition)reader.ReadUInt32();
        var majorVersion = reader.ReadUInt8();
        var minorVersion = reader.ReadUInt8();
        var buildLow = reader.ReadUInt32();
        var buildHigh = reader.ReadUInt32();
        var compilationDate = DateTime.UnixEpoch.AddMilliseconds(reader.ReadUInt64());

        return new ProductInfoTag(metadata, productId, edition, majorVersion, minorVersion, buildLow, buildHigh, compilationDate);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt32((uint)ProductId);
        writer.WriteUInt32((uint)Edition);
        writer.WriteUInt8(MajorVersion);
        writer.WriteUInt8(MinorVersion);
        writer.WriteUInt32(BuildLow);
        writer.WriteUInt32(BuildHigh);
        writer.WriteUInt64((ulong)(CompilationDate - DateTime.UnixEpoch).TotalMilliseconds);
    }
}
