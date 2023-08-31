/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace FH
{
    public static class BitUtil
    {
        public static MyBitUtil Little = new BitUtilLittle();
        public static MyBitUtil Big = new BitUtilBig();
        public static MyBitUtil Util;
        static BitUtil()
        {
            if (BitConverter.IsLittleEndian)
                Util = Little;
            Util = Big;
        }

        public static int Write(bool value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }
        public static int Write(short value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }
        public static int Write(ushort value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }
        public static int Write(int value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }
        public static int Write(uint value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }
        public static int Write(long value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }
        public static int Write(ulong value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }
        public static int Write(float value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }
        public static int Write(double value, byte[] buff, int start = 0) { return Util.Write(value, buff, start); }

        public static uint SetBit(uint tar, int idx, bool v)
        {
            //1. check
            if (idx < 0 || idx >= 32)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", 32, idx);
                return tar;
            }

            if (v)
                tar = (1u << idx) | tar;
            else
                tar = ~(1u << idx) & tar;
            return tar;
        }

        public static bool GetBit(uint tar, int idx)
        {
            //1. check
            if (idx < 0 || idx >= 32)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", 32, idx);
                return false;
            }
            return ((1u << idx) & tar) != 0;
        }


        public static uint MakePair32(ushort hi, ushort low)
        {
            BitUtilLittle.ValUnion u = new BitUtilLittle.ValUnion();
            u._u16_0 = hi;
            u._u16_1 = low;
            return u._u32_0;
        }

        public static void SplitPair32(uint v, out ushort hi, out ushort low)
        {
            BitUtilLittle.ValUnion u = new BitUtilLittle.ValUnion();
            u._u32_0 = v;
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

        public static void SplitPair(long v, out int hi, out int low)
        {
            BitUtilLittle.ValUnion u = v;
            hi = u._i32_0;
            low = u._i32_1;
        }

        public static void SplitPair(ulong v, out int hi, out int low)
        {
            BitUtilLittle.ValUnion u = v;
            hi = u._i32_0;
            low = u._i32_1;
        }

        public static void SplitPair(ulong v, out uint hi, out uint low)
        {
            BitUtilLittle.ValUnion u = v;
            hi = u._u32_0;
            low = u._u32_1;
        }

        public static bool IsPowOf2(long t)
        {
            if (t <= 0)
                return false;
            return (t & (t - 1)) == 0;
        }

        public static int LastIndexOf1(long t)
        {
            if (t <= 0)
                return -1;
            for (int i = 0; i < 64; i++)
            {
                t = t >> 1;
                if (t == 0)
                    return i;
            }
            return -1;
        }

        public static uint Enum2Uint<T>(T v) where T : Enum
        {
            Log.Assert(typeof(T).IsEnum || typeof(T).IsValueType, "T 不是enum {0}", typeof(T));
            return (uint)v.GetHashCode();
        }

        public static int Enum2Int<T>(T v) where T : Enum
        {
            Log.Assert(typeof(T).IsEnum || typeof(T).IsValueType, "T 不是enum {0}", typeof(T));
            return v.GetHashCode();
        }

        public static uint Struct2Uint<T>(T v) where T : struct
        {
            Log.Assert(typeof(T).IsEnum || typeof(T).IsValueType, "T 不是enum {0}", typeof(T));
            return (uint)v.GetHashCode();
        }

        public static int Struct2Int<T>(T v) where T : struct
        {
            Log.Assert(typeof(T).IsEnum || typeof(T).IsValueType, "T 不是enum {0}", typeof(T));
            return v.GetHashCode();
        }
    }

    public abstract class MyBitUtil
    {
        public abstract int Write(bool value, byte[] buff, int start = 0);
        public abstract int Write(short value, byte[] buff, int start = 0);
        public abstract int Write(ushort value, byte[] buff, int start = 0);
        public abstract int Write(int value, byte[] buff, int start = 0);
        public abstract int Write(uint value, byte[] buff, int start = 0);
        public abstract int Write(long value, byte[] buff, int start = 0);
        public abstract int Write(ulong value, byte[] buff, int start = 0);
        public abstract int Write(float value, byte[] buff, int start = 0);
        public abstract int Write(double value, byte[] buff, int start = 0);

        public abstract uint ToUInt32(byte[] buff, int start = 0);
        public abstract uint ToUInt32(int value);
        public abstract int ToInt32(uint value);
        public abstract ulong ToUInt64(long value);
        public abstract long ToInt64(ulong value);

        public abstract long ToInt64(double value);
        public abstract int ToInt32(float value);

    }

    public class BitUtilLittle : MyBitUtil
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
        public override long ToInt64(double value)
        {
            ValUnion v = new ValUnion();
            v._f64 = value;
            return v._i64;
        }

        public override int ToInt32(float value)
        {
            ValUnion v = new ValUnion();
            v._f32_0 = value;
            return v._i32_0;
        }

        public override uint ToUInt32(int value)
        {
            ValUnion v = new ValUnion();
            v._i32_0 = value;
            return v._u32_0;
        }
        public override int ToInt32(uint value)
        {
            ValUnion v = new ValUnion();
            v._u32_0 = value;
            return v._i32_0;
        }
        public override ulong ToUInt64(long value)
        {
            ValUnion v = new ValUnion();
            v._i64 = value;
            return v._u64;
        }
        public override long ToInt64(ulong value)
        {
            ValUnion v = new ValUnion();
            v._u64 = value;
            return v._i64;
        }

        public override uint ToUInt32(byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._u8_0 = buff[start];
            start++;
            v._u8_1 = buff[start];
            start++;
            v._u8_2 = buff[start];
            start++;
            v._u8_3 = buff[start];
            return v._u32_0;
        }

        public override int Write(bool value, byte[] buff, int start = 0)
        {
            buff[start] = value ? (byte)1 : (byte)0;
            return 1;
        }

        public override int Write(short value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._i16_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            return 2;
        }

        public override int Write(ushort value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._u16_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            return 2;
        }

        public override int Write(int value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._i32_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            return 4;
        }

        public override int Write(uint value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._u32_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            return 4;
        }

        public override int Write(long value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._i64 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            buff[start] = v._u8_4;
            start++;
            buff[start] = v._u8_5;
            start++;
            buff[start] = v._u8_6;
            start++;
            buff[start] = v._u8_7;
            start++;
            return 8;
        }

        public override int Write(ulong value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._u64 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            buff[start] = v._u8_4;
            start++;
            buff[start] = v._u8_5;
            start++;
            buff[start] = v._u8_6;
            start++;
            buff[start] = v._u8_7;
            start++;
            return 8;
        }

        public override int Write(float value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._f32_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            return 4;
        }

        public override int Write(double value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._f64 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            buff[start] = v._u8_4;
            start++;
            buff[start] = v._u8_5;
            start++;
            buff[start] = v._u8_6;
            start++;
            buff[start] = v._u8_7;
            start++;
            return 8;
        }
    }

    public class BitUtilBig : MyBitUtil
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

        public override long ToInt64(double value)
        {
            ValUnion v = new ValUnion();
            v._f64 = value;
            return v._i64;
        }

        public override int ToInt32(float value)
        {
            ValUnion v = new ValUnion();
            v._f32_0 = value;
            return v._i32_0;
        }

        public override uint ToUInt32(byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._u8_0 = buff[start];
            start++;
            v._u8_1 = buff[start];
            start++;
            v._u8_2 = buff[start];
            start++;
            v._u8_3 = buff[start];
            return v._u32_0;
        }

        public override uint ToUInt32(int value)
        {
            ValUnion v = new ValUnion();
            v._i32_0 = value;
            return v._u32_0;
        }
        public override int ToInt32(uint value)
        {
            ValUnion v = new ValUnion();
            v._u32_0 = value;
            return v._i32_0;
        }
        public override ulong ToUInt64(long value)
        {
            ValUnion v = new ValUnion();
            v._i64 = value;
            return v._u64;
        }
        public override long ToInt64(ulong value)
        {
            ValUnion v = new ValUnion();
            v._u64 = value;
            return v._i64;
        }

        public override int Write(bool value, byte[] buff, int start = 0)
        {
            buff[start] = value ? (byte)1 : (byte)0;
            return 1;
        }

        public override int Write(short value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._i16_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            return 2;
        }

        public override int Write(ushort value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._u16_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            return 2;
        }

        public override int Write(int value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._i32_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            return 4;
        }

        public override int Write(uint value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._u32_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            return 4;
        }

        public override int Write(long value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._i64 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            buff[start] = v._u8_4;
            start++;
            buff[start] = v._u8_5;
            start++;
            buff[start] = v._u8_6;
            start++;
            buff[start] = v._u8_7;
            start++;
            return 8;
        }

        public override int Write(ulong value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._u64 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            buff[start] = v._u8_4;
            start++;
            buff[start] = v._u8_5;
            start++;
            buff[start] = v._u8_6;
            start++;
            buff[start] = v._u8_7;
            start++;
            return 8;
        }

        public override int Write(float value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._f32_0 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            return 4;
        }

        public override int Write(double value, byte[] buff, int start = 0)
        {
            ValUnion v = new ValUnion();
            v._f64 = value;
            buff[start] = v._u8_0;
            start++;
            buff[start] = v._u8_1;
            start++;
            buff[start] = v._u8_2;
            start++;
            buff[start] = v._u8_3;
            start++;
            buff[start] = v._u8_4;
            start++;
            buff[start] = v._u8_5;
            start++;
            buff[start] = v._u8_6;
            start++;
            buff[start] = v._u8_7;
            start++;
            return 8;
        }
    }
}
