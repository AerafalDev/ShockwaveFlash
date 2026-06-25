
namespace ShockwaveFlash.Tags.Font;

public sealed record DefineFont4Tag(TagMetadata Metadata, ushort Id, bool IsItalic, bool IsBold, string Name, ReadOnlyMemory<byte> Data) : Tag(Metadata)
{
    public static DefineFont4Tag Decode(MemoryReader reader, TagMetadata metadata)
    {
        var id = reader.ReadUInt16();
        var flags = reader.ReadUInt8();
        var hasFontData = (flags & 4) is not 0;
        var isItalic = (flags & 2) is not 0;
        var isBold = (flags & 1) is not 0;
        var name = reader.ReadNullTerminatedString();
        var data = hasFontData ? reader.ReadMemoryToEnd() : ReadOnlyMemory<byte>.Empty;

        return new DefineFont4Tag(metadata, id, isItalic, isBold, name, data);
    }

    public override void Encode(MemoryWriter writer, byte swfVersion)
    {
        var hasFontData = !Data.IsEmpty;

        byte flags = 0;

        if (hasFontData)
            flags |= 4;

        if (IsItalic)
            flags |= 2;

        if (IsBold)
            flags |= 1;

        writer.WriteUInt16(Id);
        writer.WriteUInt8(flags);
        writer.WriteNullTerminatedString(Name);

        if (hasFontData)
            writer.WriteMemory(Data);
    }
}
