/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public struct Bit128 : IEquatable<Bit128> 
    {
        public const int LENGTH = 128;
        private const int C_COMP_LEN = 64;
        public static Bit128 All = new Bit128 (ulong.MaxValue, ulong.MaxValue);
        public static Bit128 Zero = new Bit128 (0, 0);

        private ulong _lo;
        private ulong _hi;

        public Bit128(ulong lo, ulong hi) { _lo = lo; _hi = hi; }
          

        public void SetValue(Bit128  mask, Bit128  value)
        {
            _lo = (_lo & (~mask._lo)) | (value._lo & mask._lo);
            _hi = (_hi & (~mask._hi)) | (value._hi & mask._hi);
        }

        public Bit128  GetValue(Bit128  mask)
        {
            ulong lo = _lo & mask._lo;
            ulong hi = _hi & mask._hi;
            return new Bit128 (lo, hi);
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


        public static Bit128 operator &(Bit128 a, Bit128 b)
        {
            return new Bit128(a._lo & b._lo, a._hi & b._hi);
        }

        public static Bit128 operator |(Bit128 a, Bit128 b)
        {
            return new Bit128(a._lo | b._lo, a._hi | b._hi);
        }

        public static bool operator ==(Bit128 a, Bit128 b)
        {
            return a._lo == b._lo && a._hi == b._hi;
        }

        public static bool operator !=(Bit128 a, Bit128 b)
        {
            return a._lo != b._lo || a._hi != b._hi;
        }

        public bool Equals(Bit128  other)
        {
            return _lo == other._lo && _hi == other._hi;
        } 

        public override int GetHashCode()
        {
            return HashCode.Combine(_lo, _hi);

            //int lo = (int)_lo ^ (int)(_lo >> 32);
            //int hi = (int)_hi ^ (int)(_hi >> 32);
            //return (lo * 397) ^ hi;
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
