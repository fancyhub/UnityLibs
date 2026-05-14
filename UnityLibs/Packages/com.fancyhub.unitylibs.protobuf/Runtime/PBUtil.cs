/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2019/8/7
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Runtime.CompilerServices;
namespace FH
{
    internal static class PBUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MakeTag(int field_index, EPBWireType wire_type)
        {
            return ((uint)field_index) << 3 | ((uint)wire_type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int field_index, EPBWireType wireType) SplitTag(uint tag)
        {
            if (tag == 0)
                return (0, EPBWireType.None);

            return ((int)(tag >> 3), (EPBWireType)(tag & 7));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong EncodeZigzag64(long n)
        {
            return (ulong)((n << 1) ^ (n >> 63));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long DecodeZigzag64(ulong n)
        {
            return (long)(n >> 1) ^ -(long)(n & 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint EncodeZigzag32(int n)
        {
            // Note:  the right-shift must be arithmetic
            return (uint)((n << 1) ^ (n >> 31));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecodeZigzag32(uint n)
        {
            return (int)(n >> 1) ^ -(int)(n & 1);
        }
    }
}
