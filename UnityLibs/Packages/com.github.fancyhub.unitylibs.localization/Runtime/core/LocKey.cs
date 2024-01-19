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
    public struct LocId : IEquatable<LocId>, IEqualityComparer<LocId>
    {
        public static readonly IEqualityComparer<LocId> EqualityComparer = new LocId();

        public readonly int Key;
        public LocId(int id) { this.Key = id; }

        public bool IsValid() { return Key != 0; }
        public bool Equals(LocId other) { return this.Key == other.Key; }

        public override int GetHashCode() { return Key; }

        bool IEqualityComparer<LocId>.Equals(LocId x, LocId y) { return x.Key == y.Key; }
        int IEqualityComparer<LocId>.GetHashCode(LocId obj) { return obj.Key; }
    }

    [Serializable]
    public struct LocKey : IEquatable<LocKey>, IEqualityComparer<LocKey>
    {
        public static readonly IEqualityComparer<LocKey> EqualityComparer = new LocKey();

        public string Key;
        public LocKey(string id) { this.Key = id; }

        public bool IsValid() { return !string.IsNullOrEmpty(this.Key); }

        public bool Equals(LocKey other) { return this.Key == other.Key; }
        public override int GetHashCode() { return string.IsNullOrEmpty(this.Key) ? 0 : Key.GetHashCode(); }

        bool IEqualityComparer<LocKey>.Equals(LocKey x, LocKey y) { return x.Key == y.Key; }
        int IEqualityComparer<LocKey>.GetHashCode(LocKey obj) { return string.IsNullOrEmpty(obj.Key) ? 0 : obj.Key.GetHashCode(); }
    }

    public static class LocKeyUtil
    {
        public static LocId ToLocId(this ref LocKey self)
        {
            return ToLocId(self.Key);
        }

        public static LocId ToLocId(this string value)
        {
            unchecked
            {
                // check for degenerate input
                if (string.IsNullOrEmpty(value))
                    return new LocId(0);

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
                return new LocId((int)hash);
            }
        }
    }
}