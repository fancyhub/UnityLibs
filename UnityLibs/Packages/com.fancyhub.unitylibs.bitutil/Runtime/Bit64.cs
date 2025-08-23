/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    [Serializable]
    public struct Bit64 : IEquatable<Bit64>
    {
        public const int LENGTH = 64;
        public static Bit64 All = new Bit64(ulong.MaxValue);
        public static Bit64 Zero = new Bit64(0);

        private ulong _v;

        public Bit64(ulong value) { _v = value; }
        public Bit64(long value) { _v = (ulong)value; }

        public bool SetBit(int idx, bool state)
        {
            //1. check
            if (idx < 0 || idx >= LENGTH)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", LENGTH, idx);
                return false;
            }

            if (state)
                _v = (1ul << idx) | _v;
            else
                _v = ~(1ul << idx) & _v;
            return true;
        }

        public bool GetBit(int idx)
        {
            //1. check
            if (idx < 0 || idx >= LENGTH)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", LENGTH, idx);
                return false;
            }
            return ((1ul << idx) & _v) != 0;
        }

        public bool this[int idx]
        {
            set { SetBit(idx, value); }
            get { return GetBit(idx); }
        }

        public int Length { get { return LENGTH; } }

        public int LastIndexOf(bool v)
        {
            return LastIndexOf(LENGTH - 1, v);
        }

        public int LastIndexOf(int offset, bool v)
        {
            if (offset >= LENGTH)
                offset = LENGTH - 1;
            ulong u64_v1 = v ? 1ul : 0ul;
            for (int i = offset; i >= 0; --i)
            {
                ulong u64_v2 = (_v >> i) & 1ul;
                if (u64_v2 == u64_v1)
                    return i;
            }
            return -1;
        }
        public int IndexOf(int offset, bool v)
        {
            if (offset < 0)
                return -1;
            ulong u64_v1 = v ? 1ul : 0ul;
            for (int i = offset; i < LENGTH; ++i)
            {
                ulong u64_v2 = (_v >> i) & 1ul;
                if (u64_v2 == u64_v1)
                    return i;
            }
            return -1;
        }

        public int IndexOf(bool v)
        {
            return IndexOf(0, v);
        }

        public void SetValue(Bit64 mask, Bit64 value)
        {
            _v = (_v & (~mask.Value)) | (value.Value & mask.Value);
        }

        public Bit64 GetValue(Bit64 mask)
        {
            return _v & mask.Value;
        }

        /// <summary>
        /// 清零或是置为最大值
        /// </summary>
        public void Clear(bool state)
        {
            _v = state ? ulong.MaxValue : 0;
        }

        public int GetCount(bool v)
        {
            int ret = 0;
            ulong u64_v1 = v ? 1ul : 0ul;
            for (int i = 0; i < LENGTH; ++i)
            {
                ulong u64_v2 = (_v >> i) & 1ul;
                if (u64_v2 == u64_v1)
                    ret++;
            }
            return ret;
        }

        public ulong Value { get { return _v; } set { _v = value; } }

        public bool Equals(Bit64 other) { return _v == other._v; }

        public override string ToString() { return $"0x{_v:X}"; }

        public override bool Equals(object obj)
        {
            if (obj is Bit64 other) return _v == other._v;
            if (obj is int i) return (long)_v == i;
            if (obj is long i64) return _v == (ulong)i64;
            if (obj is uint ui) return _v == ui;
            if (obj is ulong ui64) return _v == ui64;
            return false;
        }

        public override int GetHashCode() { return HashCode.Combine(_v); }

        public static Bit64 operator &(Bit64 a, Bit64 b) { return new Bit64(a._v & b._v); }
        public static Bit64 operator |(Bit64 a, Bit64 b) { return new Bit64(a._v | b._v); }
        public static bool operator ==(Bit64 a, Bit64 b) { return a._v == b._v; }
        public static bool operator !=(Bit64 a, Bit64 b) { return a._v != b._v; }

        public static implicit operator Bit64(long v) { return new Bit64(v); }
        public static implicit operator Bit64(ulong v) { return new Bit64(v); }
        public static implicit operator ulong(Bit64 v) { return v._v; }
        public static explicit operator long(Bit64 v) { return (long)(v._v); }
    }
}
