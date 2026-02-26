// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace ShockwaveFlash.Tags;

public sealed record MetadataTag(TagMetadata Metadata, [StringSyntax(StringSyntaxAttribute.Xml)] string XmlMetadata) : Tag(Metadata)
{
    public static MetadataTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        return new MetadataTag(metadata, reader.ReadNullTerminatedString());
    }
}
