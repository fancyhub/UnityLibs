/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace FH
{
    public interface EndianBitUtil
    {
        public int Write(bool value, byte[] dest_buff, int buff_offset = 0);
        public int Write(short value, byte[] dest_buff, int buff_offset = 0);
        public int Write(ushort value, byte[] dest_buff, int buff_offset = 0);
        public int Write(int value, byte[] dest_buff, int buff_offset = 0);
        public int Write(uint value, byte[] dest_buff, int buff_offset = 0);
        public int Write(long value, byte[] dest_buff, int buff_offset = 0);
        public int Write(ulong value, byte[] dest_buff, int buff_offset = 0);
        public int Write(float value, byte[] dest_buff, int buff_offset = 0);
        public int Write(double value, byte[] dest_buff, int buff_offset = 0);

        public uint ToUInt32(byte[] src_buff, int buff_offset = 0);
        public uint ToUInt32(int value);
        public int ToInt32(uint value);
        public ulong ToUInt64(long value);
        public long ToInt64(ulong value);

        public long ToInt64(double value);
        public int ToInt32(float value);
    }

    public static class BitUtil
    {
        public readonly static EndianBitUtil Little = new LittleEndianBitUtil();
        public readonly static EndianBitUtil Big = new BigEndianBitUtil();
        public readonly static EndianBitUtil CurEndian;

        static BitUtil()
        {
            if (BitConverter.IsLittleEndian)
                CurEndian = Little;
            CurEndian = Big;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Write(bool value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Write(short value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Write(ushort value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }
        public static int Write(int value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }
        public static int Write(uint value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }
        public static int Write(long value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }
        public static int Write(ulong value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }
        public static int Write(float value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }
        public static int Write(double value, byte[] dest_buff, int buff_offset = 0) { return CurEndian.Write(value, dest_buff, buff_offset); }

        public static uint SetBit(uint value, int idx, bool bit_value)
        {
            //1. check
            if (idx < 0 || idx >= 32)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", 32, idx);
                return value;
            }

            if (bit_value)
                value = (1u << idx) | value;
            else
                value = ~(1u << idx) & value;
            return value;
        }

        public static bool GetBit(uint value, int idx)
        {
            //1. check
            if (idx < 0 || idx >= 32)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", 32, idx);
                return false;
            }
            return ((1u << idx) & value) != 0;
        }


        public static uint MakePair32(ushort hi, ushort low)
        {
            LittleEndianBitUtil.ValUnion u = new LittleEndianBitUtil.ValUnion();
            u._u16_0 = hi;
            u._u16_1 = low;
            return u._u32_0;
        }

        public static void SplitPair32(uint value, out ushort hi, out ushort low)
        {
            LittleEndianBitUtil.ValUnion u = new LittleEndianBitUtil.ValUnion();
            u._u32_0 = value;
            hi = u._u16_0;
            low = u._u16_1;
        }

        public static ulong MakePair(int hi, int low)
        {
            return MakePair((uint)hi, (uint)low);
        }

        public static ulong MakePair(uint hi, uint low)
        {
            ulong ret = hi;
            ret <<= 32;
            ret |= low;
            return ret;
        }

        public static void SplitPair(long value, out int hi, out int low)
        {
            LittleEndianBitUtil.ValUnion u = value;
            hi = u._i32_0;
            low = u._i32_1;
        }

        public static void SplitPair(ulong value, out int hi, out int low)
        {
            LittleEndianBitUtil.ValUnion u = value;
            hi = u._i32_0;
            low = u._i32_1;
        }

        public static void SplitPair(ulong value, out uint hi, out uint low)
        {
            LittleEndianBitUtil.ValUnion u = value;
            hi = u._u32_0;
            low = u._u32_1;
        }

        public static bool IsPowOf2(long value)
        {
            if (value <= 0)
                return false;
            return (value & (value - 1)) == 0;
        }

        public static int LastIndexOf1(long value)
        {
            if (value <= 0)
                return -1;
            for (int i = 0; i < 64; i++)
            {
                value = value >> 1;
                if (value == 0)
                    return i;
            }
            return -1;
        }

        public static uint Enum2Uint<T>(T value) where T : Enum
        {
            return (uint)Enum2Int<T>(value);
        }

        public static int Enum2Int<T>(T value) where T : Enum
        {
            Log.Assert(typeof(T).IsEnum || typeof(T).IsValueType, "T is not enum {0}", typeof(T));
            return value.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Struct2Uint<T>(T value) where T : struct, IConvertible
        {
            return (uint)Struct2Int<T>(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Struct2Int<T>(T value) where T : struct, IConvertible
        {
            Log.Assert(typeof(T).IsEnum || typeof(T).IsValueType, "T is not enum {0}", typeof(T));
            //return v.GetHashCode();
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.EnumToInt(value);
        }


        private sealed class LittleEndianBitUtil : EndianBitUtil
        {
            /// <summary>
            /// 用来做混合的
            /// </summary>
            [StructLayout(LayoutKind.Explicit)]
            public struct ValUnion
            {
                [FieldOffset(0)] public bool _b;

                [FieldOffset(0)] public ulong _u64;
                [FieldOffset(0)] public long _i64;

                [FieldOffset(0)] public int _i32_0;
                [FieldOffset(4)] public int _i32_1;

                [FieldOffset(0)] public uint _u32_0;
                [FieldOffset(4)] public uint _u32_1;

                [FieldOffset(0)] public double _f64;

                [FieldOffset(0)] public float _f32_0;
                [FieldOffset(4)] public float _f32_1;

                [FieldOffset(0)] public short _i16_0;
                [FieldOffset(2)] public short _i16_1;
                [FieldOffset(4)] public short _i16_2;
                [FieldOffset(6)] public short _i16_3;

                [FieldOffset(0)] public ushort _u16_0;
                [FieldOffset(2)] public ushort _u16_1;
                [FieldOffset(4)] public ushort _u16_2;
                [FieldOffset(6)] public ushort _u16_3;

                [FieldOffset(0)] public byte _u8_0;
                [FieldOffset(1)] public byte _u8_1;
                [FieldOffset(2)] public byte _u8_2;
                [FieldOffset(3)] public byte _u8_3;
                [FieldOffset(4)] public byte _u8_4;
                [FieldOffset(5)] public byte _u8_5;
                [FieldOffset(6)] public byte _u8_6;
                [FieldOffset(7)] public byte _u8_7;

                public static implicit operator ValUnion(long v)
                {
                    ValUnion ret = new ValUnion();
                    ret._i64 = v;
                    return ret;
                }

                public static implicit operator ValUnion(ulong v)
                {
                    ValUnion ret = new ValUnion();
                    ret._u64 = v;
                    return ret;
                }
            }
            public long ToInt64(double value)
            {
                ValUnion v = new ValUnion();
                v._f64 = value;
                return v._i64;
            }

            public int ToInt32(float value)
            {
                ValUnion v = new ValUnion();
                v._f32_0 = value;
                return v._i32_0;
            }

            public uint ToUInt32(int value)
            {
                ValUnion v = new ValUnion();
                v._i32_0 = value;
                return v._u32_0;
            }
            public int ToInt32(uint value)
            {
                ValUnion v = new ValUnion();
                v._u32_0 = value;
                return v._i32_0;
            }
            public ulong ToUInt64(long value)
            {
                ValUnion v = new ValUnion();
                v._i64 = value;
                return v._u64;
            }
            public long ToInt64(ulong value)
            {
                ValUnion v = new ValUnion();
                v._u64 = value;
                return v._i64;
            }

            public uint ToUInt32(byte[] src_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._u8_0 = src_buff[buff_offset];
                buff_offset++;
                v._u8_1 = src_buff[buff_offset];
                buff_offset++;
                v._u8_2 = src_buff[buff_offset];
                buff_offset++;
                v._u8_3 = src_buff[buff_offset];
                return v._u32_0;
            }

            public int Write(bool value, byte[] dest_buff, int buff_offset = 0)
            {
                dest_buff[buff_offset] = value ? (byte)1 : (byte)0;
                return 1;
            }

            public int Write(short value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._i16_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                return 2;
            }

            public int Write(ushort value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._u16_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                return 2;
            }

            public int Write(int value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._i32_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                return 4;
            }

            public int Write(uint value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._u32_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                return 4;
            }

            public int Write(long value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._i64 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_4;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_5;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_6;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_7;
                buff_offset++;
                return 8;
            }

            public int Write(ulong value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._u64 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_4;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_5;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_6;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_7;
                buff_offset++;
                return 8;
            }

            public int Write(float value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._f32_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                return 4;
            }

            public int Write(double value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._f64 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_4;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_5;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_6;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_7;
                buff_offset++;
                return 8;
            }
        }


        private sealed class BigEndianBitUtil : EndianBitUtil
        {
            /// <summary>
            /// 用来做混合的
            /// </summary>
            [StructLayout(LayoutKind.Explicit)]
            public struct ValUnion
            {
                [FieldOffset(0)] public ulong _u64;
                [FieldOffset(0)] public long _i64;

                [FieldOffset(4)] public int _i32_0;
                [FieldOffset(0)] public int _i32_1;

                [FieldOffset(4)] public uint _u32_0;
                [FieldOffset(0)] public uint _u32_1;

                [FieldOffset(0)] public double _f64;

                [FieldOffset(4)] public float _f32_0;
                [FieldOffset(0)] public float _f32_1;

                [FieldOffset(6)] public short _i16_0;
                [FieldOffset(4)] public short _i16_1;
                [FieldOffset(2)] public short _i16_2;
                [FieldOffset(0)] public short _i16_3;

                [FieldOffset(6)] public ushort _u16_0;
                [FieldOffset(4)] public ushort _u16_1;
                [FieldOffset(2)] public ushort _u16_2;
                [FieldOffset(0)] public ushort _u16_3;

                [FieldOffset(7)] public byte _u8_0;
                [FieldOffset(6)] public byte _u8_1;
                [FieldOffset(5)] public byte _u8_2;
                [FieldOffset(4)] public byte _u8_3;
                [FieldOffset(3)] public byte _u8_4;
                [FieldOffset(2)] public byte _u8_5;
                [FieldOffset(1)] public byte _u8_6;
                [FieldOffset(0)] public byte _u8_7;

                public static implicit operator ValUnion(long v)
                {
                    ValUnion ret = new ValUnion();
                    ret._i64 = v;
                    return ret;
                }

                public static implicit operator ValUnion(ulong v)
                {
                    ValUnion ret = new ValUnion();
                    ret._u64 = v;
                    return ret;
                }
            }

            public long ToInt64(double value)
            {
                ValUnion v = new ValUnion();
                v._f64 = value;
                return v._i64;
            }

            public int ToInt32(float value)
            {
                ValUnion v = new ValUnion();
                v._f32_0 = value;
                return v._i32_0;
            }

            public uint ToUInt32(byte[] src_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._u8_0 = src_buff[buff_offset];
                buff_offset++;
                v._u8_1 = src_buff[buff_offset];
                buff_offset++;
                v._u8_2 = src_buff[buff_offset];
                buff_offset++;
                v._u8_3 = src_buff[buff_offset];
                return v._u32_0;
            }

            public uint ToUInt32(int value)
            {
                ValUnion v = new ValUnion();
                v._i32_0 = value;
                return v._u32_0;
            }
            public int ToInt32(uint value)
            {
                ValUnion v = new ValUnion();
                v._u32_0 = value;
                return v._i32_0;
            }
            public ulong ToUInt64(long value)
            {
                ValUnion v = new ValUnion();
                v._i64 = value;
                return v._u64;
            }
            public long ToInt64(ulong value)
            {
                ValUnion v = new ValUnion();
                v._u64 = value;
                return v._i64;
            }

            public int Write(bool value, byte[] dest_buff, int buff_offset = 0)
            {
                dest_buff[buff_offset] = value ? (byte)1 : (byte)0;
                return 1;
            }

            public int Write(short value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._i16_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                return 2;
            }

            public int Write(ushort value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._u16_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                return 2;
            }

            public int Write(int value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._i32_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                return 4;
            }

            public int Write(uint value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._u32_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                return 4;
            }

            public int Write(long value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._i64 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_4;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_5;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_6;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_7;
                buff_offset++;
                return 8;
            }

            public int Write(ulong value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._u64 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_4;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_5;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_6;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_7;
                buff_offset++;
                return 8;
            }

            public int Write(float value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._f32_0 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                return 4;
            }

            public int Write(double value, byte[] dest_buff, int buff_offset = 0)
            {
                ValUnion v = new ValUnion();
                v._f64 = value;
                dest_buff[buff_offset] = v._u8_0;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_1;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_2;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_3;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_4;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_5;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_6;
                buff_offset++;
                dest_buff[buff_offset] = v._u8_7;
                buff_offset++;
                return 8;
            }
        }
    }
}
