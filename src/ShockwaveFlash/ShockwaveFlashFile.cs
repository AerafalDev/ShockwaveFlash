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
        var body = new MemoryWriter();

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
}
