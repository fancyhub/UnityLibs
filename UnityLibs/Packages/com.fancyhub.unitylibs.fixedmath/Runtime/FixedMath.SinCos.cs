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
        public static readonly Fixed32 Pi = new Fixed32(3142);
        public static readonly Fixed32 PiTimes2 = new Fixed32(6283); //Pi*2;
        public static readonly Fixed32 PiOver2 = new Fixed32(1571); //Pi/2;
        public static readonly Fixed32 PiOver4 = new Fixed32(785); //Pi/4;

        public const int SinCosTableSize = 1024;

        private static readonly Fixed32 Atan2C0 = Fixed32.FromDouble(-0.0464964749);
        private static readonly Fixed32 Atan2C1 = Fixed32.FromDouble(-0.243647006);
        private static readonly Fixed32 Atan2C2 = Fixed32.FromDouble(-0.204625908);
        private static readonly Fixed32 Atan2C3 = Fixed32.FromDouble(1.57079637);

        private static readonly int[] SinTable = BuildSinTable();

        private static int[] BuildSinTable()
        {
            var t = new int[SinCosTableSize];
            for (int i = 0; i < SinCosTableSize; i++)
            {
                double a = (i * (Math.PI * 2.0)) / SinCosTableSize;
                t[i] = (int)(Math.Sin(a) * Fixed32.OneRaw);
            }
            return t;
        }           

        public static Fixed32 Sin(Fixed32 rad)
        {
            Fixed32 x = ModTwoPi(rad);
            long idx = ((long)x.Raw * SinCosTableSize) / PiTimes2.Raw;
            int i = (int)idx;
            if (i >= SinCosTableSize) i = SinCosTableSize - 1;
            int i1 = (i + 1) % SinCosTableSize;
            Fixed32 frac = x - Fixed32.FromRaw((int)((long)i * PiTimes2.Raw / SinCosTableSize));
            Fixed32 step = Fixed32.FromRaw(PiTimes2.Raw / SinCosTableSize);
            Fixed32 t = step.Raw != 0 ? frac / step : Fixed32.Zero;
            Fixed32 s0 = Fixed32.FromRaw(SinTable[i]);
            Fixed32 s1 = Fixed32.FromRaw(SinTable[i1]);
            return LerpUnclamped(s0, s1, t);
        }

        public static Fixed32 Cos(Fixed32 rad)
        {
            return Sin(rad + PiOver2);
        }

        public static Fixed32 Tan(Fixed32 rad)
        {
            Fixed32 c = Cos(rad);
            if (c.Raw == 0) return Fixed32.FromInt(0);
            return Sin(rad) / c;
        }
         
        /// <summary>Approximate atan2; returns radians in (-π, π].</summary>
        public static Fixed32 Atan2(Fixed32 y, Fixed32 x)
        {
            if (x.Raw == 0 && y.Raw == 0) return Fixed32.Zero;

            Fixed32 ax = Abs(x);
            Fixed32 ay = Abs(y);
            Fixed32 a = Min(ax, ay) / Max(ax, ay);
            Fixed32 s = a * a;
            Fixed32 r = Atan2C0 * s;
            r = (Atan2C1 + r) * s;
            r = (Atan2C2 + r) * s;
            r = (Atan2C3 + r) * a;
            if (ay.Raw > ax.Raw) r = PiOver2 - r;
            if (x.Raw < 0) r = Pi - r;
            if (y.Raw < 0) r = -r;
            return r;
        }

        public static Fixed32 Asin(Fixed32 x)
        {
            x = Clamp(x, Fixed32.MinusOne, Fixed32.One);
            Fixed32 y = Sqrt(Fixed32.One - x * x);
            return Atan2(x, y);
        }

        public static Fixed32 Acos(Fixed32 x)
        {
            x = Clamp(x, Fixed32.MinusOne, Fixed32.One);
            Fixed32 y = Sqrt(Fixed32.One - x * x);
            return Atan2(y, x);
        }
         
    }
}
