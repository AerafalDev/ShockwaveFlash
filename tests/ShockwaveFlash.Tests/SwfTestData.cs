using System.IO.Compression;

namespace ShockwaveFlash.Tests;

internal static class SwfTestData
{
    public static byte[] ToZlibCompressed(byte[] uncompressedFws)
    {
        var body = uncompressedFws[8..];

        using var ms = new MemoryStream();

        using (var zs = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            zs.Write(body, 0, body.Length);

        var compressedBody = ms.ToArray();

        var result = new byte[8 + compressedBody.Length];
        uncompressedFws.AsSpan(0, 8).CopyTo(result);
        result[0] = (byte)'C';
        compressedBody.CopyTo(result, 8);

        return result;
    }


    public static byte[] MinimalUncompressed()
    {
        byte[] body =
        [
            0x08, 0x00,
            0x00, 0x18,
            0x01, 0x00,
            0x43, 0x02, 0xFF, 0x00, 0x00,
            0x40, 0x00,
            0x00, 0x00,
        ];

        return WithHeader('F', version: 6, body);
    }

    public static byte[] WithDoAction()
    {
        byte[] body =
        [
            0x08, 0x00,
            0x00, 0x18,
            0x01, 0x00,
            0x03, 0x03,
            0x06,
            0x07,
            0x00,
            0x00, 0x00,
        ];

        return WithHeader('F', version: 6, body);
    }

    private static byte[] WithHeader(char signature, byte version, byte[] body)
    {
        var fileLength = 8 + body.Length;

        byte[] file =
        [
            (byte)signature, (byte)'W', (byte)'S',
            version,
            (byte)(fileLength & 0xFF),
            (byte)((fileLength >> 8) & 0xFF),
            (byte)((fileLength >> 16) & 0xFF),
            (byte)((fileLength >> 24) & 0xFF),
            .. body,
        ];

        return file;
    }
}
