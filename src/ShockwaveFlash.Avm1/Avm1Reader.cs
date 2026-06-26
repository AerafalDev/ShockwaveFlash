namespace ShockwaveFlash.Avm1;

internal static class Avm1Reader
{
    public static double ReadDouble(MemoryReader reader)
    {
        var high = reader.ReadUInt32();
        var low = reader.ReadUInt32();

        return BitConverter.Int64BitsToDouble((long)(((ulong)high << 32) | low));
    }

    public static void WriteDouble(MemoryWriter writer, double value)
    {
        var bits = (ulong)BitConverter.DoubleToInt64Bits(value);

        writer.WriteUInt32((uint)(bits >> 32));
        writer.WriteUInt32((uint)bits);
    }
}
