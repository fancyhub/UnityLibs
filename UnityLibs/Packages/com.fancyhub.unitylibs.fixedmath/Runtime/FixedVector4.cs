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
    public struct FixedVector4 : IEquatable<FixedVector4>
    {
        public Fixed32 X;
        public Fixed32 Y;
        public Fixed32 Z;
        public Fixed32 W;

        public static readonly FixedVector4 Zero = default;
        public static readonly FixedVector4 One = new FixedVector4(Fixed32.One, Fixed32.One, Fixed32.One, Fixed32.One);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedVector4(Fixed32 x, Fixed32 y, Fixed32 z, Fixed32 w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Dot(FixedVector4 a, FixedVector4 b) =>
            a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 SqrMagnitude(FixedVector4 v) => Dot(v, v);

        public static Fixed32 Magnitude(FixedVector4 v) => FixedMath.Sqrt(SqrMagnitude(v));

        public static FixedVector4 Normalize(FixedVector4 v)
        {
            Fixed32 m = Magnitude(v);
            if (m.Raw == 0) return Zero;
            return v / m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector4 operator +(FixedVector4 a, FixedVector4 b) =>
            new FixedVector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector4 operator -(FixedVector4 a, FixedVector4 b) =>
            new FixedVector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector4 operator *(FixedVector4 a, Fixed32 s) =>
            new FixedVector4(a.X * s, a.Y * s, a.Z * s, a.W * s);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector4 operator *(Fixed32 s, FixedVector4 a) => a * s;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector4 operator /(FixedVector4 a, Fixed32 s) =>
            new FixedVector4(a.X / s, a.Y / s, a.Z / s, a.W / s);

        public bool Equals(FixedVector4 other) =>
            X == other.X && Y == other.Y && Z == other.Z && W == other.W;

        public override bool Equals(object obj) => obj is FixedVector4 other && Equals(other);

        public override int GetHashCode()
        {
            return System.HashCode.Combine(X.Raw, Y.Raw, Z.Raw, W.Raw);
        }

        public override string ToString() =>
            $"({X.ToFloat():G}, {Y.ToFloat():G}, {Z.ToFloat():G}, {W.ToFloat():G})";
    }
}
