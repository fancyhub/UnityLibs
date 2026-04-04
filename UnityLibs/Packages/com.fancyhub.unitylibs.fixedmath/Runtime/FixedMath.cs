/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/03/01
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace FH
{
    public static partial class FixedMath
    {         
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Abs(Fixed32 v)
        {
            int r = v.Raw;
            int mask = r >> 31;
            return Fixed32.FromRaw((r + mask) ^ mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Min(Fixed32 a, Fixed32 b) => a.Raw < b.Raw ? a : b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Max(Fixed32 a, Fixed32 b) => a.Raw > b.Raw ? a : b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Clamp(Fixed32 v, Fixed32 min, Fixed32 max)
        {
            if (v.Raw < min.Raw) return min;
            if (v.Raw > max.Raw) return max;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Clamp01(Fixed32 v) => Clamp(v, Fixed32.Zero, Fixed32.One);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Lerp(Fixed32 a, Fixed32 b, Fixed32 t) =>
            LerpUnclamped(a, b, Clamp01(t));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 LerpUnclamped(Fixed32 a, Fixed32 b, Fixed32 t) =>
            a + (b - a) * t;

        /// <summary>Angle in radians, normalized to [0, 2π).</summary>
        public static Fixed32 ModTwoPi(Fixed32 rad)
        {
            long twoPi = PiTimes2.Raw;
            long r = rad.Raw;
            r %= twoPi;
            if (r < 0) r += twoPi;
            return Fixed32.FromRaw((int)r);
        }
         

        /// <summary>Integer sqrt for non-negative 64-bit value (floor).</summary>
        public static long SqrtLong(long n)
        {
            if (n <= 0) return 0;
            long x = n;
            long y = (x + 1) >> 1;
            while (y < x)
            {
                x = y;
                y = (x + n / x) >> 1;
            }
            return x;
        }

        public static Fixed32 Sqrt(Fixed32 v)
        {
            if (v.Raw <= 0) return Fixed32.Zero;
            long n = (long)v.Raw * Fixed32.Precision;
            if (n < 0) return Fixed32.Zero;
            long s = SqrtLong(n);
            return Fixed32.FromRaw((int)s);
        }
         
        public static Fixed32 Pow(Fixed32 baseVal, int exp)
        {
            if (exp == 0) return Fixed32.One;
            if (exp < 0) return Fixed32.One / Pow(baseVal, -exp);
            Fixed32 r = Fixed32.One;
            Fixed32 b = baseVal;
            while (exp > 0)
            {
                if ((exp & 1) != 0) r *= b;
                b *= b;
                exp >>= 1;
            }
            return r;
        }
    }
}
