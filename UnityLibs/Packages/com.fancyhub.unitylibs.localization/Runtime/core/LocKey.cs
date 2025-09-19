/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace FH
{
    [Serializable]
    public struct LocKeyInt : IEquatable<LocKeyInt>, IEqualityComparer<LocKeyInt>
    {
        public static readonly IEqualityComparer<LocKeyInt> EqualityComparer = new LocKeyInt();

        public readonly int Key;
        public LocKeyInt(int int_key) { this.Key = int_key; }

        public bool IsValid() { return Key != 0; }
        public bool Equals(LocKeyInt other) { return this.Key == other.Key; }

        public override int GetHashCode() { return Key; }

        bool IEqualityComparer<LocKeyInt>.Equals(LocKeyInt x, LocKeyInt y) { return x.Key == y.Key; }
        int IEqualityComparer<LocKeyInt>.GetHashCode(LocKeyInt obj) { return obj.Key; }
    }

    [Serializable]
    public struct LocKeyStr : IEquatable<LocKeyStr>, IEqualityComparer<LocKeyStr>
    {
        public static readonly IEqualityComparer<LocKeyStr> EqualityComparer = new LocKeyStr();

        public string Key;
        public LocKeyStr(string str_key) { this.Key = str_key; }

        public bool IsValid() { return !string.IsNullOrEmpty(this.Key); }

        public bool Equals(LocKeyStr other) { return this.Key == other.Key; }
        public override int GetHashCode() { return string.IsNullOrEmpty(this.Key) ? 0 : Key.GetHashCode(); }

        bool IEqualityComparer<LocKeyStr>.Equals(LocKeyStr x, LocKeyStr y) { return x.Key == y.Key; }
        int IEqualityComparer<LocKeyStr>.GetHashCode(LocKeyStr obj) { return string.IsNullOrEmpty(obj.Key) ? 0 : obj.Key.GetHashCode(); }
    }

    public static class LocKeyUtil
    {
        public static LocKeyInt ToLocId(this ref LocKeyStr self)
        {
            return ToLocId(self.Key);
        }

        /// <remarks>Based on <a href="http://www.azillionmonkeys.com/qed/hash.html">SuperFastHash</a>.</remarks>
        public static LocKeyInt ToLocId(this string value)
        {
            unchecked
            {
                // check for degenerate input
                if (string.IsNullOrEmpty(value))
                    return new LocKeyInt(0);

                int length = value.Length;
                uint hash = (uint)length;

                int remainder = length & 1;
                length >>= 1;

                // main loop
                int index = 0;
                for (; length > 0; length--)
                {
                    hash += value[index];
                    uint temp = (uint)(value[index + 1] << 11) ^ hash;
                    hash = (hash << 16) ^ temp;
                    index += 2;
                    hash += hash >> 11;
                }

                // handle odd string length
                if (remainder == 1)
                {
                    hash += value[index];
                    hash ^= hash << 11;
                    hash += hash >> 17;
                }

                // force "avalanching" of final 127 bits
                hash ^= hash << 3;
                hash += hash >> 5;
                hash ^= hash << 4;
                hash += hash >> 17;
                hash ^= hash << 25;
                hash += hash >> 6;
                return new LocKeyInt((int)hash);
            }
        }
    }
}