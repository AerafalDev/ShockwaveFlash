using ShockwaveFlash.Types.Control;

namespace ShockwaveFlash.Tags.Control;

public sealed record SymbolClassTag(TagMetadata Metadata, SymbolReference[] Symbols) : Tag(Metadata)
{
    public static SymbolClassTag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var numSymbols = reader.ReadUInt16();
        var symbols = new SymbolReference[numSymbols];

        for (var i = 0; i < numSymbols; i++)
            symbols[i] = new SymbolReference(reader.ReadUInt16(), reader.ReadNullTerminatedString());

        return new SymbolClassTag(metadata, symbols);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        writer.WriteUInt16((ushort)Symbols.Length);

        foreach (var symbol in Symbols)
        {
            writer.WriteUInt16(symbol.Id);
            writer.WriteNullTerminatedString(symbol.Name);
        }
    }
}
