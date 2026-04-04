/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/03/01
 * Title   : 
 * Desc    : 千分位定点：Raw 为毫单位，1.0 = Scale raw，即小数精度 0.001。运算用乘除 Scale，不用移位。
*************************************************************************************/

using System;
using System.Runtime.CompilerServices;

namespace FH
{
    [Serializable]
    public struct Fixed32 : IEquatable<Fixed32>, IComparable<Fixed32>
    {
        /// <summary>比例：1.0 对应的 raw 值（千分位，三位小数）。</summary>
        internal const int Precision = 1000;

        internal const int OneRaw = Precision;

        public int Raw;

        public static readonly Fixed32 Zero = default;
        public static readonly Fixed32 One = new Fixed32(OneRaw);
        public static readonly Fixed32 MinusOne = new Fixed32(-OneRaw);
        public static readonly Fixed32 Half = new Fixed32(OneRaw / 2);
        public static readonly Fixed32 Two = new Fixed32(OneRaw * 2);

        

        public static readonly Fixed32 MaxValue = new Fixed32(int.MaxValue);
        public static readonly Fixed32 MinValue = new Fixed32(int.MinValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fixed32(int raw)
        {
            Raw = raw;
        }

        #region Convert

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Fixed32 FromRaw(int raw) => new Fixed32(raw);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Fixed32 FromInt(int v) => new Fixed32(v * OneRaw);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Fixed32 FromFloat(float v) { return new Fixed32((int)(v * OneRaw)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Fixed32 FromDouble(double v) { return new Fixed32((int)(v * OneRaw)); }

        /// <summary>向零截断。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public int ToInt() => Raw / OneRaw;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public float ToFloat() => Raw / (float)OneRaw;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public double ToDouble() => Raw / (double)OneRaw;


        public static explicit operator float(Fixed32 ob) { return ob.ToFloat(); }
        public static explicit operator Fixed32(float f) { return FromFloat(f); }
        public static explicit operator int(Fixed32 ob) { return ob.ToInt(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CeilToInt()
        {
            if (Raw >= 0)
            {
                int rem = Raw % OneRaw;
                if (rem == 0) return Raw / OneRaw;
                return Raw / OneRaw + 1;
            }
            return Raw / OneRaw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FloorToInt()
        {
            if (Raw >= 0) return Raw / OneRaw;
            int rem = Raw % OneRaw;
            if (rem == 0) return Raw / OneRaw;
            return Raw / OneRaw - 1;
        }

        #endregion


        #region op +-*/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 operator +(Fixed32 a, Fixed32 b) => new Fixed32(a.Raw + b.Raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 operator -(Fixed32 a, Fixed32 b) => new Fixed32(a.Raw - b.Raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 operator -(Fixed32 a) => new Fixed32(-a.Raw);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 operator *(Fixed32 a, Fixed32 b)
        {
            return new Fixed32((int)(((long)a.Raw * b.Raw) / Precision));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 operator /(Fixed32 a, Fixed32 b)
        {
            return new Fixed32((int)(((long)a.Raw * Precision) / b.Raw));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 operator %(Fixed32 a, Fixed32 b)
        {
            return FromInt(a.Raw % b.Raw);
        }

        public static Fixed32 operator *(float a, Fixed32 b) { return FromFloat(a) * b; }
        public static Fixed32 operator *(Fixed32 a, float b) { return FromFloat(b) * a; }
        public static Fixed32 operator *(int a, Fixed32 b) { return FromInt(a) * b; }
        public static Fixed32 operator *(Fixed32 a, int b) { return FromInt(b) * a; }
        public static Fixed32 operator +(int a, Fixed32 b) { return FromInt(a) + b; }
        public static Fixed32 operator +(Fixed32 a, int b) { return FromInt(b) + a; }
        #endregion

        #region logic

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fixed32 a, Fixed32 b) => a.Raw == b.Raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fixed32 a, Fixed32 b) => a.Raw != b.Raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Fixed32 a, Fixed32 b) => a.Raw < b.Raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Fixed32 a, Fixed32 b) => a.Raw > b.Raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Fixed32 a, Fixed32 b) => a.Raw <= b.Raw;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Fixed32 a, Fixed32 b) => a.Raw >= b.Raw;

        public bool Equals(Fixed32 other) => Raw == other.Raw;

        public override bool Equals(object obj) => obj is Fixed32 other && Equals(other);

        public int CompareTo(Fixed32 other) => Raw.CompareTo(other.Raw);

        #endregion


        public override int GetHashCode() => Raw;
        public override string ToString() => ToFloat().ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
