using System.Diagnostics.CodeAnalysis;

namespace ShockwaveFlash.Tags.Metadata;

public sealed record MetadataTag(TagMetadata Metadata, [StringSyntax(StringSyntaxAttribute.Xml)] string XmlMetadata) : Tag(Metadata)
{
    public static MetadataTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        return new MetadataTag(metadata, reader.ReadNullTerminatedString());
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteNullTerminatedString(XmlMetadata);
    }
}
