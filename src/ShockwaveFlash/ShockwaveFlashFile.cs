using ShockwaveFlash.Exceptions;
using ShockwaveFlash.Tags;

namespace ShockwaveFlash;

public sealed record ShockwaveFlashFile(ShockwaveFlashHeader Header, IReadOnlyList<Tag> Tags)
{
    private const int HeaderSize = 8;

    public static ShockwaveFlashFile Disassemble(ReadOnlyMemory<byte> data)
    {
        var reader = new MemoryReader(data);

        var compression = (ShockwaveFlashCompression)reader.ReadUInt8();

        if (compression is not (ShockwaveFlashCompression.None or ShockwaveFlashCompression.ZLib or ShockwaveFlashCompression.Lzma))
            throw new SwfFormatException($"Invalid SWF signature byte 0x{(byte)compression:X2}; expected 'F', 'C' or 'Z'.");

        if (reader.ReadUInt8() is not (byte)'W' || reader.ReadUInt8() is not (byte)'S')
            throw new SwfFormatException("Invalid SWF signature; expected 'WS' after the compression byte.");

        var version = reader.ReadUInt8();
        var fileLength = reader.ReadInt32();
        var bodyLength = fileLength - HeaderSize;

        reader = new MemoryReader(compression.Decompress(reader.ReadMemoryToEnd(), bodyLength));

        var header = ShockwaveFlashHeader.Decode(reader, compression, version, fileLength);

        var tags = Tag.DecodeCollection(reader, version);

        return new ShockwaveFlashFile(header, tags);
    }

    public ReadOnlyMemory<byte> Assemble()
    {
        var body = new MemoryWriter(EstimateBodyCapacity());

        Header.Encode(body);
        Tag.EncodeCollection(body, Tags, Header.Version);

        var compressedBody = Header.Compression.Compress(body.WrittenMemory);
        var fileLength = HeaderSize + body.Position;

        var file = new MemoryWriter(HeaderSize + compressedBody.Length);

        file.WriteUInt8((byte)Header.Compression);
        file.WriteUInt8((byte)'W');
        file.WriteUInt8((byte)'S');
        file.WriteUInt8(Header.Version);
        file.WriteInt32(fileLength);
        file.WriteMemory(compressedBody);

        return file.WrittenMemory;
    }

    public ShockwaveFlashFile ReplaceTag(Tag oldTag, Tag newTag)
    {
        return ReplaceTagAt(IndexOf(oldTag), newTag);
    }

    public ShockwaveFlashFile ReplaceTagAt(int index, Tag newTag)
    {
        var tags = Tags.ToList();
        tags[index] = newTag;
        return this with { Tags = tags };
    }

    public ShockwaveFlashFile InsertTag(int index, Tag tag)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Tags.Count);

        var tags = Tags.ToList();
        tags.Insert(index, tag);
        return this with { Tags = tags };
    }

    public ShockwaveFlashFile AppendTag(Tag tag)
    {
        return InsertTag(Tags.Count, tag);
    }

    public ShockwaveFlashFile RemoveTag(Tag tag)
    {
        return RemoveTagAt(IndexOf(tag));
    }

    public ShockwaveFlashFile RemoveTagAt(int index)
    {
        var tags = Tags.ToList();
        tags.RemoveAt(index);
        return this with { Tags = tags };
    }

    public ShockwaveFlashFile MoveTag(int fromIndex, int toIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fromIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fromIndex, Tags.Count);
        ArgumentOutOfRangeException.ThrowIfNegative(toIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(toIndex, Tags.Count);

        var tags = Tags.ToList();
        var tag = tags[fromIndex];
        tags.RemoveAt(fromIndex);
        tags.Insert(toIndex, tag);
        return this with { Tags = tags };
    }

    private int IndexOf(Tag tag)
    {
        for (var i = 0; i < Tags.Count; i++)
            if (ReferenceEquals(Tags[i], tag))
                return i;

        throw new ArgumentException("The tag was not found in this file.", nameof(tag));
    }

    private int EstimateBodyCapacity()
    {
        long estimate = 64;

        foreach (var tag in Tags)
            estimate += tag.Metadata.Length + 6;

        return estimate > Array.MaxLength ? Array.MaxLength : (int)estimate;
    }
}
