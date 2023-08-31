/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    /// <summary>
    /// 对应的枚举 不要本身就是 Flags类型的
    /// </summary>
    public struct BitEnum128<T> : IEquatable<BitEnum128<T>> where T : Enum
    {
        public const int LENGTH = 128;
        private const int C_COMP_LEN = 64;
        public static BitEnum128<T> All = new BitEnum128<T>(ulong.MaxValue, ulong.MaxValue);
        public static BitEnum128<T> Zero = new BitEnum128<T>(0, 0);

        private ulong _lo;
        private ulong _hi;
        public BitEnum128(ulong lo, ulong hi) { _lo = lo; _hi = hi; }

        public bool SetBit(T idx, bool state)
        {
            //1. check
            int index = idx.GetHashCode();
            return SetBit(index, state);
        }

        public bool GetBit(T idx)
        {
            //1. check
            int index = idx.GetHashCode();
            return GetBit(index);
        }

        public void SetValue(BitEnum128<T> mask, BitEnum128<T> value)
        {
            _lo = (_lo & (~mask._lo)) | (value._lo & mask._lo);
            _hi = (_hi & (~mask._hi)) | (value._hi & mask._hi);
        }

        public BitEnum128<T> GetValue(BitEnum128<T> mask)
        {
            ulong lo = _lo & mask._lo;
            ulong hi = _hi & mask._hi;
            return new BitEnum128<T>(lo, hi);
        }

        public static BitEnum128<T> operator &(BitEnum128<T> a, BitEnum128<T> b)
        {
            return new BitEnum128<T>(a._lo & b._lo, a._hi & b._hi);
        }

        public static BitEnum128<T> operator |(BitEnum128<T> a, BitEnum128<T> b)
        {
            return new BitEnum128<T>(a._lo | b._lo, a._hi | b._hi);
        }

        public static bool operator ==(BitEnum128<T> a, BitEnum128<T> b)
        {
            return a._lo == b._lo && a._hi == b._hi;
        }

        public static bool operator !=(BitEnum128<T> a, BitEnum128<T> b)
        {
            return a._lo != b._lo || a._hi != b._hi;
        }

        public bool IsZero()
        {
            return _lo == 0 && _hi == 0;
        }

        public bool SetBit(int idx, bool state)
        {
            //1. check            
            if (idx < 0 || idx >= LENGTH)
            {
                Log.Assert(false, "idx:{1} 要在 [0,{0})", LENGTH, idx);
                return false;
            }

            if (idx < C_COMP_LEN)
            {
                if (state)
                    _lo |= (1ul << idx);
                else
                    _lo &= ~(1ul << idx);
                return true;
            }

            idx -= C_COMP_LEN;
            if (state)
                _hi |= (1ul << idx);
            else
                _hi &= ~(1ul << idx);
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

            if (idx < C_COMP_LEN)
                return ((1ul << idx) & _lo) != 0;

            idx -= C_COMP_LEN;
            return ((1ul << idx) & _hi) != 0;
        }

        public bool this[T idx]
        {
            set
            {
                int index = idx.GetHashCode();
                SetBit(index, value);
            }
            get
            {
                int index = idx.GetHashCode();
                return GetBit(index);
            }
        }

        public bool this[int idx]
        {
            set { SetBit(idx, value); }
            get { return GetBit(idx); }
        }

        public int Length { get { return LENGTH; } }

        /// <summary>
        /// 清零或是置为最大值
        /// </summary>
        public void Clear(bool state)
        {
            _hi = state ? ulong.MaxValue : 0;
            _lo = _hi;
        }

        public ulong ValueLow { get { return _lo; } set { _lo = value; } }
        public ulong ValueHi { get { return _hi; } set { _hi = value; } }

        public int GetCount(bool v)
        {
            int ret = 0;
            ulong u64_v1 = v ? 1ul : 0ul;

            for (int i = 0; i < C_COMP_LEN; ++i)
            {
                ulong u64_v2 = (_lo >> i) & 0x1ul;
                if (u64_v2 == u64_v1)
                    ret++;
            }

            for (int i = 0; i < C_COMP_LEN; ++i)
            {
                ulong u64_v2 = (_hi >> i) & 0x1ul;
                if (u64_v2 == u64_v1)
                    ret++;
            }
            return ret;
        }

        public bool Equals(BitEnum128<T> other)
        {
            return _lo == other._lo && _hi == other._hi;
        }

        public override int GetHashCode()
        {
            int lo = (int)_lo ^ (int)(_lo >> 32);
            int hi = (int)_hi ^ (int)(_hi >> 32);
            return (lo * 397) ^ hi;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
