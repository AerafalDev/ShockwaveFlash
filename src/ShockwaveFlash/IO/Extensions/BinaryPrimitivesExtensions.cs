using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace ShockwaveFlash.IO.Extensions;

internal static class BinaryPrimitivesExtensions
{
    extension(BinaryPrimitives)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt24LittleEndian(in ReadOnlySpan<byte> source)
        {
            return source[0] | ((uint)source[1] << 8) | ((uint)source[2] << 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt24LittleEndian(in ReadOnlySpan<byte> source)
        {
            return ((source[0] | (source[1] << 8) | (source[2] << 16)) << 8) >> 8;
        }
    }
}
