/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2020/5/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{ 
    public enum EResType
    {
        None,
        Res,
        Sprite,
        Inst,
        EmptyInst,
    }

    public readonly struct ResId : IEquatable<ResId>, IEqualityComparer<ResId>
    {
        public static ResId Null = new ResId();

        public readonly int Id;
        public readonly EResType ResType;

        public ResId(int id, EResType resType) { Id = id; ResType = resType; }

        public ResId(UnityEngine.Object obj, EResType resType) { Id = obj == null ? 0 : obj.GetInstanceID(); ResType = resType; }

        public bool Equals(ResId other) { return Id == other.Id && ResType == other.ResType; }

        public bool IsValid() { return Id != 0 && ResType != EResType.None; }

        public override int GetHashCode() { return System.HashCode.Combine(Id, ResType); }

        public override bool Equals(object obj)
        {
            if (obj is ResId other) return Id == other.Id && ResType == other.ResType;
            return false;
        }

        public override string ToString() { return $"{ResType}:{Id}"; }

        public static bool operator ==(ResId left, ResId right) { return left.Id == right.Id && left.ResType == right.ResType; }
        public static bool operator !=(ResId left, ResId right) { return left.Id != right.Id || left.ResType != right.ResType; }

        bool IEqualityComparer<ResId>.Equals(ResId x, ResId y) { return x.Id == y.Id && x.ResType == y.ResType; }

        int IEqualityComparer<ResId>.GetHashCode(ResId obj) { return obj.GetHashCode(); }
    }     
}
