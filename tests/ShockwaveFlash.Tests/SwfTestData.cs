// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tests;

internal static class SwfTestData
{
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
