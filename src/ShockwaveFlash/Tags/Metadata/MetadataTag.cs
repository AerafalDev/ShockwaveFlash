using System.Diagnostics.CodeAnalysis;

namespace ShockwaveFlash.Tags.Metadata;

public sealed class MetadataTag : Tag
{
    [StringSyntax(StringSyntaxAttribute.Xml)]
    public string XmlMetadata { get; set; }

    public MetadataTag(TagMetadata metadata, [StringSyntax(StringSyntaxAttribute.Xml)] string xmlMetadata) : base(metadata)
    {
        XmlMetadata = xmlMetadata;
    }

    public static MetadataTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new MetadataTag(metadata, reader.ReadNullTerminatedString());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteNullTerminatedString(XmlMetadata);
    }
}
