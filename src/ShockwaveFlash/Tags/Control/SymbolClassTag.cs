// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Control;

public sealed record SymbolClassTag(TagMetadata Metadata, SymbolReference[] Symbols) : Tag(Metadata)
{
    public static SymbolClassTag Decode(ref SpanReader reader, TagMetadata metadata)
    {
        var numSymbols = reader.ReadUInt16();
        var symbols = new SymbolReference[numSymbols];

        for (var i = 0; i < numSymbols; i++)
            symbols[i] = new SymbolReference(reader.ReadUInt16(), reader.ReadNullTerminatedString());

        return new SymbolClassTag(metadata, symbols);
    }
}
