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
    [Serializable]
    public struct FixedQuaternion : IEquatable<FixedQuaternion>
    {
        public Fixed32 X;
        public Fixed32 Y;
        public Fixed32 Z;
        public Fixed32 W;

        public static readonly FixedQuaternion Identity = new FixedQuaternion(
            Fixed32.Zero, Fixed32.Zero, Fixed32.Zero, Fixed32.One);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedQuaternion(Fixed32 x, Fixed32 y, Fixed32 z, Fixed32 w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>Rotation around axis (must be unit); angle in radians.</summary>
        public static FixedQuaternion AngleAxis(Fixed32 angleRad, FixedVector3 axis)
        {
            Fixed32 ha = angleRad * Fixed32.Half;
            Fixed32 s = FixedMath.Sin(ha);
            Fixed32 c = FixedMath.Cos(ha);
            FixedVector3 n = FixedVector3.Normalize(axis);
            return new FixedQuaternion(n.X * s, n.Y * s, n.Z * s, c);
        }

        /// <summary>Same axis order as Unity Quaternion.Euler: first Z, then X, then Y (degrees).</summary>
        public static FixedQuaternion EulerDegrees(Fixed32 xDeg, Fixed32 yDeg, Fixed32 zDeg)
        {
            Fixed32 degToRad = FixedMath.Pi / Fixed32.FromInt(180);
            Fixed32 xr = xDeg * degToRad;
            Fixed32 yr = yDeg * degToRad;
            Fixed32 zr = zDeg * degToRad;
            FixedQuaternion qz = AngleAxis(zr, FixedVector3.UnitZ);
            FixedQuaternion qx = AngleAxis(xr, FixedVector3.UnitX);
            FixedQuaternion qy = AngleAxis(yr, FixedVector3.UnitY);
            return qy * (qx * qz);
        }

        public static FixedQuaternion Normalize(FixedQuaternion q)
        {
            Fixed32 n = FixedMath.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
            if (n.Raw == 0) return Identity;
            return new FixedQuaternion(q.X / n, q.Y / n, q.Z / n, q.W / n);
        }

        public static FixedQuaternion Inverse(FixedQuaternion q)
        {
            Fixed32 n2 = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
            if (n2.Raw == 0) return Identity;
            Fixed32 inv = Fixed32.One / n2;
            return new FixedQuaternion(-q.X * inv, -q.Y * inv, -q.Z * inv, q.W * inv);
        }

        public static FixedQuaternion Lerp(FixedQuaternion a, FixedQuaternion b, Fixed32 t)
        {
            return Normalize(new FixedQuaternion(
                FixedMath.Lerp(a.X, b.X, t),
                FixedMath.Lerp(a.Y, b.Y, t),
                FixedMath.Lerp(a.Z, b.Z, t),
                FixedMath.Lerp(a.W, b.W, t)));
        }

        public static FixedQuaternion LerpUnclamped(FixedQuaternion a, FixedQuaternion b, Fixed32 t)
        {
            return Normalize(new FixedQuaternion(
                FixedMath.LerpUnclamped(a.X, b.X, t),
                FixedMath.LerpUnclamped(a.Y, b.Y, t),
                FixedMath.LerpUnclamped(a.Z, b.Z, t),
                FixedMath.LerpUnclamped(a.W, b.W, t)));
        }

        public static FixedQuaternion Slerp(FixedQuaternion a, FixedQuaternion b, Fixed32 t)
        {
            t = FixedMath.Clamp01(t);
            Fixed32 dot = a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
            FixedQuaternion b2 = b;
            if (dot.Raw < 0)
            {
                b2 = new FixedQuaternion(-b.X, -b.Y, -b.Z, -b.W);
                dot = -dot;
            }
            if (dot.Raw >= Fixed32.OneRaw - 8)
                return LerpUnclamped(a, b2, t);

            Fixed32 theta = FixedMath.Acos(FixedMath.Clamp(dot, Fixed32.MinusOne, Fixed32.One));
            Fixed32 sinTheta = FixedMath.Sin(theta);
            if (sinTheta.Raw < 8)
                return LerpUnclamped(a, b2, t);

            Fixed32 inv = Fixed32.One / sinTheta;
            Fixed32 wa = FixedMath.Sin((Fixed32.One - t) * theta) * inv;
            Fixed32 wb = FixedMath.Sin(t * theta) * inv;
            return Normalize(new FixedQuaternion(
                a.X * wa + b2.X * wb,
                a.Y * wa + b2.Y * wb,
                a.Z * wa + b2.Z * wb,
                a.W * wa + b2.W * wb));
        }

        public static FixedVector3 Rotate(FixedQuaternion q, FixedVector3 v)
        {
            FixedQuaternion p = new FixedQuaternion(v.X, v.Y, v.Z, Fixed32.Zero);
            FixedQuaternion qInv = Inverse(q);
            FixedQuaternion r = q * p * qInv;
            return new FixedVector3(r.X, r.Y, r.Z);
        }

        public static FixedQuaternion operator *(FixedQuaternion a, FixedQuaternion b)
        {
            return new FixedQuaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
        }

        public bool Equals(FixedQuaternion other) =>
            X == other.X && Y == other.Y && Z == other.Z && W == other.W;

        public override bool Equals(object obj) => obj is FixedQuaternion other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = X.Raw;
                h = (h * 397) ^ Y.Raw;
                h = (h * 397) ^ Z.Raw;
                h = (h * 397) ^ W.Raw;
                return h;
            }
        }

        public override string ToString() =>
            $"({X.ToFloat():G}, {Y.ToFloat():G}, {Z.ToFloat():G}, {W.ToFloat():G})";
    }
}
