namespace ShockwaveFlash.Avm1;

internal static class Avm1Reader
{
    public static double ReadDouble(MemoryReader reader)
    {
        var high = reader.ReadUInt32();
        var low = reader.ReadUInt32();

        return BitConverter.Int64BitsToDouble((long)(((ulong)high << 32) | low));
    }
}
