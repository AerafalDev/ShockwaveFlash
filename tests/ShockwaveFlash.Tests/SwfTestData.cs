// Copyright (c) Aerafal 2026.
// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace ShockwaveFlash.Tests;

/// <summary>
/// Hand-built minimal SWF byte blobs used as fixtures. Layout follows
/// <c>swf-spec-19.pdf</c> §2 (SWF header) and §2 (Tag format / RECORDHEADER).
/// </summary>
internal static class SwfTestData
{
    /// <summary>
    /// Uncompressed (<c>FWS</c>) SWF, version 6, 24 fps, 1 frame, with a red
    /// <c>SetBackgroundColor</c>, a <c>ShowFrame</c> and an <c>End</c> tag.
    /// </summary>
    public static byte[] MinimalUncompressed()
    {
        byte[] body =
        [
            // FrameSize RECT: Nbits = 1, Xmin/Xmax/Ymin/Ymax = 0  (§1 Rectangle record)
            0x08, 0x00,
            // FrameRate: 8.8 fixed = 24.0  (0x1800 little-endian)
            0x00, 0x18,
            // FrameCount: 1
            0x01, 0x00,
            // SetBackgroundColor (code 9, length 3) -> RECORDHEADER 0x0243; RGB = red
            0x43, 0x02, 0xFF, 0x00, 0x00,
            // ShowFrame (code 1, length 0)
            0x40, 0x00,
            // End (code 0, length 0)
            0x00, 0x00,
        ];

        return WithHeader('F', version: 6, body);
    }

    /// <summary>
    /// Uncompressed SWF whose single <c>DoAction</c> tag carries Play, Stop and End
    /// actions (§5). Exercises the deferred <see cref="System.ReadOnlyMemory{T}"/>
    /// payload path: actions decode without re-supplying the source buffer.
    /// </summary>
    public static byte[] WithDoAction()
    {
        byte[] body =
        [
            0x08, 0x00,             // FrameSize RECT (Nbits = 1, zeros)
            0x00, 0x18,             // FrameRate 24.0
            0x01, 0x00,             // FrameCount 1
            // DoAction (code 12, length 3) -> RECORDHEADER 0x0303
            0x03, 0x03,
            0x06,                   // ActionPlay
            0x07,                   // ActionStop
            0x00,                   // ActionEnd
            0x00, 0x00,             // End tag
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
