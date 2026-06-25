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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt24LittleEndian(in Span<byte> destination, uint value)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
            destination[2] = (byte)(value >> 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt24LittleEndian(in Span<byte> destination, int value)
        {
            destination[0] = (byte)value;
            destination[1] = (byte)(value >> 8);
            destination[2] = (byte)(value >> 16);
        }
    }
}
