namespace ShockwaveFlash.Types.Sound;

public enum AudioCompression : byte
{
    UncompressedUnknownEndian = 0,
    Adpcm = 1,
    Mp3 = 2,
    Uncompressed = 3,
    Nellymoser16Khz = 4,
    Nellymoser8Khz = 5,
    Nellymoser = 6,
    Aac = 10,
    Speex = 11
}
