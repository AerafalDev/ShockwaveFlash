using SkiaSharp;

namespace ShockwaveFlash.Rendering.Drawing.Skia;

internal static class GifEncoder
{
    private const int MaxColors = 256;

    private const byte AlphaThreshold = 128;

    public static byte[] Encode(IReadOnlyList<SKColor[]> frames, int width, int height, int delayCentiseconds)
    {
        var (palette, transparentIndex) = BuildPalette(frames);
        var lookup = new Dictionary<uint, int>();

        for (var i = 0; i < palette.Count; i++)
            lookup[Key(palette[i])] = i;

        using var stream = new MemoryStream();

        WriteHeader(stream, width, height, palette);
        WriteLoop(stream);

        foreach (var frame in frames)
        {
            var indices = MapFrame(frame, palette, lookup, transparentIndex);
            WriteFrame(stream, width, height, indices, delayCentiseconds, transparentIndex, palette.Count);
        }

        stream.WriteByte(0x3B);
        return stream.ToArray();
    }

    private static (List<SKColor> Palette, int TransparentIndex) BuildPalette(IReadOnlyList<SKColor[]> frames)
    {
        var counts = new Dictionary<uint, int>();
        var hasTransparency = false;

        foreach (var frame in frames)
        {
            foreach (var pixel in frame)
            {
                if (pixel.Alpha < AlphaThreshold)
                {
                    hasTransparency = true;
                    continue;
                }

                var key = Key(pixel);
                counts[key] = counts.GetValueOrDefault(key) + 1;
            }
        }

        var budget = hasTransparency ? MaxColors - 1 : MaxColors;
        var colors = counts.Count <= budget
            ? counts.Keys.Select(FromKey).ToList()
            : MedianCut(counts, budget);

        if (colors.Count == 0)
            colors.Add(new SKColor(0, 0, 0));

        var transparentIndex = -1;

        if (hasTransparency)
        {
            transparentIndex = colors.Count;
            colors.Add(new SKColor(0, 0, 0));
        }

        return (colors, transparentIndex);
    }

    private static List<SKColor> MedianCut(Dictionary<uint, int> counts, int budget)
    {
        var colors = counts.Keys.ToList();
        var boxes = new List<List<uint>> { colors };

        while (boxes.Count < budget)
        {
            var index = LargestBox(boxes);

            if (index < 0)
                break;

            var box = boxes[index];
            var axis = LongestAxis(box);

            box.Sort((a, b) => Channel(a, axis).CompareTo(Channel(b, axis)));

            var mid = box.Count / 2;
            boxes[index] = box.GetRange(0, mid);
            boxes.Add(box.GetRange(mid, box.Count - mid));
        }

        return boxes.Where(static box => box.Count > 0).Select(box => Average(box, counts)).ToList();
    }

    private static int LargestBox(List<List<uint>> boxes)
    {
        var best = -1;
        var bestRange = 0;

        for (var i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Count < 2)
                continue;

            var range = ColorRange(boxes[i]);

            if (range > bestRange)
            {
                bestRange = range;
                best = i;
            }
        }

        return best;
    }

    private static int LongestAxis(List<uint> box)
    {
        int rMin = 255, rMax = 0, gMin = 255, gMax = 0, bMin = 255, bMax = 0;

        foreach (var color in box)
        {
            var r = Channel(color, 0);
            var g = Channel(color, 1);
            var b = Channel(color, 2);
            rMin = Math.Min(rMin, r);
            rMax = Math.Max(rMax, r);
            gMin = Math.Min(gMin, g);
            gMax = Math.Max(gMax, g);
            bMin = Math.Min(bMin, b);
            bMax = Math.Max(bMax, b);
        }

        var dr = rMax - rMin;
        var dg = gMax - gMin;
        var db = bMax - bMin;

        return dr >= dg && dr >= db ? 0 : dg >= db ? 1 : 2;
    }

    private static int ColorRange(List<uint> box)
    {
        int rMin = 255, rMax = 0, gMin = 255, gMax = 0, bMin = 255, bMax = 0;

        foreach (var color in box)
        {
            var r = Channel(color, 0);
            var g = Channel(color, 1);
            var b = Channel(color, 2);
            rMin = Math.Min(rMin, r);
            rMax = Math.Max(rMax, r);
            gMin = Math.Min(gMin, g);
            gMax = Math.Max(gMax, g);
            bMin = Math.Min(bMin, b);
            bMax = Math.Max(bMax, b);
        }

        return Math.Max(rMax - rMin, Math.Max(gMax - gMin, bMax - bMin));
    }

    private static SKColor Average(List<uint> box, Dictionary<uint, int> counts)
    {
        long r = 0, g = 0, b = 0, total = 0;

        foreach (var color in box)
        {
            var weight = counts[color];
            r += (long)Channel(color, 0) * weight;
            g += (long)Channel(color, 1) * weight;
            b += (long)Channel(color, 2) * weight;
            total += weight;
        }

        total = Math.Max(total, 1);
        return new SKColor((byte)(r / total), (byte)(g / total), (byte)(b / total));
    }

    private static byte[] MapFrame(SKColor[] frame, List<SKColor> palette, Dictionary<uint, int> lookup, int transparentIndex)
    {
        var indices = new byte[frame.Length];

        for (var i = 0; i < frame.Length; i++)
        {
            var pixel = frame[i];

            if (pixel.Alpha < AlphaThreshold && transparentIndex >= 0)
            {
                indices[i] = (byte)transparentIndex;
                continue;
            }

            indices[i] = (byte)(lookup.TryGetValue(Key(pixel), out var exact) ? exact : Nearest(pixel, palette, transparentIndex));
        }

        return indices;
    }

    private static int Nearest(SKColor pixel, List<SKColor> palette, int transparentIndex)
    {
        var best = 0;
        var bestDistance = int.MaxValue;

        for (var i = 0; i < palette.Count; i++)
        {
            if (i == transparentIndex)
                continue;

            var dr = pixel.Red - palette[i].Red;
            var dg = pixel.Green - palette[i].Green;
            var db = pixel.Blue - palette[i].Blue;
            var distance = (dr * dr) + (dg * dg) + (db * db);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return best;
    }

    private static void WriteHeader(MemoryStream stream, int width, int height, List<SKColor> palette)
    {
        var bits = ColorBits(palette.Count);

        stream.Write("GIF89a"u8);
        WriteUInt16(stream, width);
        WriteUInt16(stream, height);
        stream.WriteByte((byte)(0xF0 | (bits - 1)));
        stream.WriteByte(0);
        stream.WriteByte(0);

        var size = 1 << bits;

        for (var i = 0; i < size; i++)
        {
            var color = i < palette.Count ? palette[i] : new SKColor(0, 0, 0);
            stream.WriteByte(color.Red);
            stream.WriteByte(color.Green);
            stream.WriteByte(color.Blue);
        }
    }

    private static void WriteLoop(MemoryStream stream)
    {
        stream.WriteByte(0x21);
        stream.WriteByte(0xFF);
        stream.WriteByte(0x0B);
        stream.Write("NETSCAPE2.0"u8);
        stream.WriteByte(0x03);
        stream.WriteByte(0x01);
        WriteUInt16(stream, 0);
        stream.WriteByte(0x00);
    }

    private static void WriteFrame(MemoryStream stream, int width, int height, byte[] indices, int delayCentiseconds, int transparentIndex, int paletteCount)
    {
        stream.WriteByte(0x21);
        stream.WriteByte(0xF9);
        stream.WriteByte(0x04);
        stream.WriteByte((byte)((1 << 2) | (transparentIndex >= 0 ? 1 : 0)));
        WriteUInt16(stream, delayCentiseconds);
        stream.WriteByte((byte)(transparentIndex >= 0 ? transparentIndex : 0));
        stream.WriteByte(0x00);

        stream.WriteByte(0x2C);
        WriteUInt16(stream, 0);
        WriteUInt16(stream, 0);
        WriteUInt16(stream, width);
        WriteUInt16(stream, height);
        stream.WriteByte(0x00);

        var minCodeSize = Math.Max(2, ColorBits(paletteCount));
        stream.WriteByte((byte)minCodeSize);

        var compressed = Lzw.Compress(indices, minCodeSize);

        for (var offset = 0; offset < compressed.Length; offset += 255)
        {
            var length = Math.Min(255, compressed.Length - offset);
            stream.WriteByte((byte)length);
            stream.Write(compressed, offset, length);
        }

        stream.WriteByte(0x00);
    }

    private static int ColorBits(int colorCount)
    {
        var bits = 1;

        while (1 << bits < colorCount)
            bits++;

        return Math.Clamp(bits, 1, 8);
    }

    private static void WriteUInt16(MemoryStream stream, int value)
    {
        stream.WriteByte((byte)(value & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
    }

    private static uint Key(SKColor color)
    {
        return ((uint)color.Red << 16) | ((uint)color.Green << 8) | color.Blue;
    }

    private static SKColor FromKey(uint key)
    {
        return new SKColor((byte)(key >> 16), (byte)(key >> 8), (byte)key);
    }

    private static int Channel(uint key, int axis)
    {
        return axis switch
        {
            0 => (int)((key >> 16) & 0xFF),
            1 => (int)((key >> 8) & 0xFF),
            _ => (int)(key & 0xFF)
        };
    }
}
