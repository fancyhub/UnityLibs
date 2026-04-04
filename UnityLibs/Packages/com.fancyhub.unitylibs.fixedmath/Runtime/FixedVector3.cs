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
    public struct FixedVector3 : IEquatable<FixedVector3>
    {
        public Fixed32 X;
        public Fixed32 Y;
        public Fixed32 Z;

        public static readonly FixedVector3 Zero = default;
        public static readonly FixedVector3 One = new FixedVector3(Fixed32.One, Fixed32.One, Fixed32.One);
        public static readonly FixedVector3 UnitX = new FixedVector3(Fixed32.One, Fixed32.Zero, Fixed32.Zero);
        public static readonly FixedVector3 UnitY = new FixedVector3(Fixed32.Zero, Fixed32.One, Fixed32.Zero);
        public static readonly FixedVector3 UnitZ = new FixedVector3(Fixed32.Zero, Fixed32.Zero, Fixed32.One);
        public static readonly FixedVector3 Up = UnitY;
        public static readonly FixedVector3 Forward = UnitZ;
        public static readonly FixedVector3 Right = UnitX;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedVector3(Fixed32 x, Fixed32 y, Fixed32 z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 FromFloat(float x, float y, float z) =>
            new FixedVector3(Fixed32.FromFloat(x), Fixed32.FromFloat(y), Fixed32.FromFloat(z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Dot(FixedVector3 a, FixedVector3 b) =>
            a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 SqrMagnitude(FixedVector3 v) => Dot(v, v);

        public static Fixed32 Magnitude(FixedVector3 v) => FixedMath.Sqrt(SqrMagnitude(v));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 Lerp(FixedVector3 a, FixedVector3 b, Fixed32 t) =>
            new FixedVector3(
                FixedMath.Lerp(a.X, b.X, t),
                FixedMath.Lerp(a.Y, b.Y, t),
                FixedMath.Lerp(a.Z, b.Z, t));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 LerpUnclamped(FixedVector3 a, FixedVector3 b, Fixed32 t) =>
            new FixedVector3(
                FixedMath.LerpUnclamped(a.X, b.X, t),
                FixedMath.LerpUnclamped(a.Y, b.Y, t),
                FixedMath.LerpUnclamped(a.Z, b.Z, t));

        public static FixedVector3 Normalize(FixedVector3 v)
        {
            Fixed32 m = Magnitude(v);
            if (m.Raw == 0) return Zero;
            return v / m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 Cross(FixedVector3 a, FixedVector3 b) =>
            new FixedVector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 operator +(FixedVector3 a, FixedVector3 b) =>
            new FixedVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 operator -(FixedVector3 a, FixedVector3 b) =>
            new FixedVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 operator -(FixedVector3 a) =>
            new FixedVector3(-a.X, -a.Y, -a.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 operator *(FixedVector3 a, Fixed32 s) =>
            new FixedVector3(a.X * s, a.Y * s, a.Z * s);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 operator *(Fixed32 s, FixedVector3 a) => a * s;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector3 operator /(FixedVector3 a, Fixed32 s) =>
            new FixedVector3(a.X / s, a.Y / s, a.Z / s);

        public bool Equals(FixedVector3 other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is FixedVector3 other && Equals(other);

        public override int GetHashCode()
        {
            return System.HashCode.Combine(X.Raw, Y.Raw, Z.Raw);
        }

        public override string ToString() =>
            $"({X.ToFloat():G}, {Y.ToFloat():G}, {Z.ToFloat():G})";
    }
}
