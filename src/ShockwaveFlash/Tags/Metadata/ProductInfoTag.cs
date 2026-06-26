using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Metadata;

public sealed class ProductInfoTag : Tag
{
    private static readonly ulong MaxCompilationMilliseconds = (ulong)(DateTime.MaxValue - DateTime.UnixEpoch).TotalMilliseconds;

    public FlashProduct ProductId { get; set; }

    public FlashEdition Edition { get; set; }

    public byte MajorVersion { get; set; }

    public byte MinorVersion { get; set; }

    public uint BuildLow { get; set; }

    public uint BuildHigh { get; set; }

    public DateTime CompilationDate { get; set; }

    public ProductInfoTag(TagMetadata metadata, FlashProduct productId, FlashEdition edition, byte majorVersion, byte minorVersion, uint buildLow, uint buildHigh, DateTime compilationDate) : base(metadata)
    {
        ProductId = productId;
        Edition = edition;
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        BuildLow = buildLow;
        BuildHigh = buildHigh;
        CompilationDate = compilationDate;
    }

    public static ProductInfoTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var productId = (FlashProduct)reader.ReadUInt32();
        var edition = (FlashEdition)reader.ReadUInt32();
        var majorVersion = reader.ReadUInt8();
        var minorVersion = reader.ReadUInt8();
        var buildLow = reader.ReadUInt32();
        var buildHigh = reader.ReadUInt32();
        var milliseconds = reader.ReadUInt64();

        if (milliseconds > MaxCompilationMilliseconds)
            throw new SwfFormatException($"ProductInfo compilation date of {milliseconds} ms since the Unix epoch is outside the representable range.");

        var compilationDate = DateTime.UnixEpoch.AddMilliseconds(milliseconds);

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
