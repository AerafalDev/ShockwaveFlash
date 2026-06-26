namespace ShockwaveFlash.Rendering.Drawing.Skia;

internal static class Lzw
{
    public static byte[] Compress(byte[] indices, int minCodeSize)
    {
        var clearCode = 1 << minCodeSize;
        var endCode = clearCode + 1;
        var codeSize = minCodeSize + 1;
        var nextCode = endCode + 1;
        var table = new Dictionary<int, int>();

        var output = new List<byte>();
        var bitBuffer = 0;
        var bitCount = 0;

        void Emit(int code)
        {
            bitBuffer |= code << bitCount;
            bitCount += codeSize;

            while (bitCount >= 8)
            {
                output.Add((byte)(bitBuffer & 0xFF));
                bitBuffer >>= 8;
                bitCount -= 8;
            }
        }

        Emit(clearCode);

        if (indices.Length == 0)
        {
            Emit(endCode);

            if (bitCount > 0)
                output.Add((byte)(bitBuffer & 0xFF));

            return output.ToArray();
        }

        var current = (int)indices[0];

        for (var i = 1; i < indices.Length; i++)
        {
            var next = (int)indices[i];
            var key = (current << 8) | next;

            if (table.TryGetValue(key, out var combined))
            {
                current = combined;
                continue;
            }

            Emit(current);
            table[key] = nextCode;
            nextCode++;

            if (nextCode == 4096)
            {
                Emit(clearCode);
                table.Clear();
                codeSize = minCodeSize + 1;
                nextCode = endCode + 1;
            }
            else if (nextCode == (1 << codeSize) + 1 && codeSize < 12)
            {
                codeSize++;
            }

            current = next;
        }

        Emit(current);
        Emit(endCode);

        if (bitCount > 0)
            output.Add((byte)(bitBuffer & 0xFF));

        return output.ToArray();
    }
}
