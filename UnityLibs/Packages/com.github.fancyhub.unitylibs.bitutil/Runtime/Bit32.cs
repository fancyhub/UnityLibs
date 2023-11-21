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
    public struct Bit32 : IEquatable<Bit32>
    {
        public const int LENGTH = 32;
        public static Bit32 All = new Bit32(uint.MaxValue);
        public static Bit32 Zero = new Bit32(0);
        private uint _v;
        public Bit32(uint value) { _v = value; }
        public Bit32(int value) { _v = (uint)value; }

        public bool SetBit(int idx, bool value)
        {
            //1. check
            if (idx < 0 || idx >= LENGTH)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", LENGTH, idx);
                return false;
            }

            if (value) 
                _v = (1u << idx) | _v;
            else
                _v = ~(1u << idx) & _v;
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
            return ((1u << idx) & _v) != 0;
        }

        public bool this[int idx]
        {
            set { SetBit(idx, value); }
            get { return GetBit(idx); }
        }

        public int Length { get { return LENGTH; } }

        public int LastIndexOf(bool value)
        {
            return LastIndexOf(LENGTH - 1, value);
        }

        public int LastIndexOf(int offset, bool value)
        {
            if (offset >= LENGTH)
                offset = LENGTH - 1;
            uint u32_v1 = value ? 1u : 0u;
            for (int i = offset; i >= 0; --i)
            {
                uint u32_v2 = (_v >> i) & 0x1u;
                if (u32_v2 == u32_v1)
                    return i;
            }
            return -1;
        }
        public int IndexOf(int offset, bool value)
        {
            if (offset < 0)
                return -1;
            uint u32_v1 = value ? 1u : 0u;
            for (int i = offset; i < LENGTH; ++i)
            {
                uint u32_v2 = (_v >> i) & 0x1u;
                if (u32_v2 == u32_v1)
                    return i;
            }
            return -1;
        }

        public int IndexOf(bool value)
        {
            return IndexOf(0, value);
        }

        public void SetValue(Bit32 mask, Bit32 value)
        {
            _v = (_v & (~mask.Value)) | (value.Value & mask.Value);
        }

        public Bit32 GetValue(Bit32 mask)
        {
            return _v & mask.Value;
        }

        /// <summary>
        /// 清零或是置为最大值
        /// </summary>
        public void Clear(bool state)
        {
            _v = state ? uint.MaxValue : 0;
        }

        public int GetCount(bool v)
        {
            int ret = 0;
            uint u32_v1 = v ? 1u : 0u;
            for (int i = 0; i < LENGTH; ++i)
            {
                uint u32_v2 = (_v >> i) & 0x1u;
                if (u32_v2 == u32_v1)
                    ret++;
            }
            return ret;
        }

        public uint Value { get { return _v; } set { _v = value; } }

        public bool Equals(Bit32 other) { return _v == other._v; }

        public override string ToString() { return $"0x{_v:X}"; }

        public override int GetHashCode() { return HashCode.Combine(_v); }

        public override bool Equals(object obj)
        {
            if (obj is Bit32 other) return other._v == _v;
            if (obj is int i) return _v == (uint)i;
            if (obj is uint ui) return _v == ui;
            return false;
        }

        public static Bit32 operator &(Bit32 a, Bit32 b) { return new Bit32(a._v & b._v); }
        public static Bit32 operator |(Bit32 a, Bit32 b) { return new Bit32(a._v | b._v); }
        public static bool operator ==(Bit32 a, Bit32 b) { return a._v == b._v; }
        public static bool operator !=(Bit32 a, Bit32 b) { return a._v != b._v; }
        public static implicit operator Bit32(int v) { return new Bit32(v); }
        public static implicit operator Bit32(uint v) { return new Bit32(v); }
        public static implicit operator uint(Bit32 v) { return v._v; }
        public static explicit operator int(Bit32 v) { return (int)(v._v); }
    }
}
