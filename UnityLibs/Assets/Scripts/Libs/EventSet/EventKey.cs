/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/01
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace FH
{
    /// <summary>
    /// A Type Of Event Key
    /// </summary>
    public readonly struct EventKey : IEquatable<EventKey>
    {
        public static IEqualityComparer<EventKey> EqualityComparer = new EqualityComparerImp();

        public readonly string Name;
        public readonly int Id;

        public EventKey(string name, int id = 0) { Name = name; Id = id; }

        public override string ToString() { return $"{Name}#{Id}"; }

        public override bool Equals(object obj) { if (obj is EventKey other) return other.Name == Name && other.Id == Id; return false; }

        public bool Equals(EventKey other) { return other.Name == Name && other.Id == Id; }

        public override int GetHashCode() { return HashCode.Combine(Name, Id); }

        public static bool operator ==(EventKey left, EventKey right) { return left.Equals(right); }

        public static bool operator !=(EventKey left, EventKey right) { return !(left == right); }

        private class EqualityComparerImp : IEqualityComparer<EventKey>
        {
            public bool Equals(EventKey x, EventKey y) { return x.Equals(y); }
            public int GetHashCode(EventKey obj) { return obj.GetHashCode(); }
        }
    }
}
