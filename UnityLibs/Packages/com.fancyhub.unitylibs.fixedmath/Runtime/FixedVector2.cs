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
    public struct FixedVector2 : IEquatable<FixedVector2>
    {
        public Fixed32 X;
        public Fixed32 Y;

        public static readonly FixedVector2 Zero = default;
        public static readonly FixedVector2 One = new FixedVector2(Fixed32.One, Fixed32.One);
        public static readonly FixedVector2 UnitX = new FixedVector2(Fixed32.One, Fixed32.Zero);
        public static readonly FixedVector2 UnitY = new FixedVector2(Fixed32.Zero, Fixed32.One);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedVector2(Fixed32 x, Fixed32 y)
        {
            X = x;
            Y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 FromFloat(float x, float y) =>
            new FixedVector2(Fixed32.FromFloat(x), Fixed32.FromFloat(y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 Dot(FixedVector2 a, FixedVector2 b) => a.X * b.X + a.Y * b.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixed32 SqrMagnitude(FixedVector2 v) => Dot(v, v);

        public static Fixed32 Magnitude(FixedVector2 v) => FixedMath.Sqrt(SqrMagnitude(v));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 Lerp(FixedVector2 a, FixedVector2 b, Fixed32 t) =>
            new FixedVector2(FixedMath.Lerp(a.X, b.X, t), FixedMath.Lerp(a.Y, b.Y, t));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 LerpUnclamped(FixedVector2 a, FixedVector2 b, Fixed32 t) =>
            new FixedVector2(
                FixedMath.LerpUnclamped(a.X, b.X, t),
                FixedMath.LerpUnclamped(a.Y, b.Y, t));

        public static FixedVector2 Normalize(FixedVector2 v)
        {
            Fixed32 m = Magnitude(v);
            if (m.Raw == 0) return Zero;
            return v / m;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b) =>
            new FixedVector2(a.X + b.X, a.Y + b.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b) =>
            new FixedVector2(a.X - b.X, a.Y - b.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 operator -(FixedVector2 a) => new FixedVector2(-a.X, -a.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 operator *(FixedVector2 a, Fixed32 s) =>
            new FixedVector2(a.X * s, a.Y * s);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 operator *(Fixed32 s, FixedVector2 a) => a * s;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FixedVector2 operator /(FixedVector2 a, Fixed32 s) =>
            new FixedVector2(a.X / s, a.Y / s);

        public bool Equals(FixedVector2 other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is FixedVector2 other && Equals(other);

        public override int GetHashCode() => System.HashCode.Combine(X.Raw, Y.Raw);

        public override string ToString() => $"({X.ToFloat():G}, {Y.ToFloat():G})";
    }
}
