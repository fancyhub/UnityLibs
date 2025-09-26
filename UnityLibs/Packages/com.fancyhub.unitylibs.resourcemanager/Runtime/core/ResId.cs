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
        Asset,
        Inst,
    }

    [Serializable]
    public readonly struct ResId : IEquatable<ResId>, IEqualityComparer<ResId>
    {
        public readonly static IEqualityComparer<ResId> EqualityComparer = new ResId();
        public readonly static ResId Null = new ResId();

        public readonly int Id;
        public readonly EResType ResType;

        //internal ResId(int id, EResType resType) { Id = id; ResType = resType; }
        internal ResId(UnityEngine.Object obj, EResType resType) { Id = obj == null ? 0 : obj.GetInstanceID(); ResType = resType; }
        internal ResId(int id, EResType resType) { Id = id; ResType = resType; }

        public bool IsValid() { return Id != 0 && ResType != EResType.None; }

        public override int GetHashCode() { return System.HashCode.Combine(Id, ResType); }
        public bool Equals(ResId other) { return Id == other.Id && ResType == other.ResType; }
        public override bool Equals(object obj) { if (obj is ResId other) return Id == other.Id && ResType == other.ResType; return false; }

        public override string ToString() { return $"{ResType}:{Id}"; }

        public static bool operator ==(ResId left, ResId right) { return left.Id == right.Id && left.ResType == right.ResType; }
        public static bool operator !=(ResId left, ResId right) { return left.Id != right.Id || left.ResType != right.ResType; }

        bool IEqualityComparer<ResId>.Equals(ResId x, ResId y) { return x.Id == y.Id && x.ResType == y.ResType; }
        int IEqualityComparer<ResId>.GetHashCode(ResId obj) { return obj.GetHashCode(); }
    }

    public enum EAssetPathType
    {
        Default,
        Sprite,
        AnimClip,
        Max,
    }

    public static class AssetPathTypeExt
    {
        private static readonly System.Type[] _UnityTypes = new Type[]
        {
            typeof(UnityEngine.Object), //Default
            typeof(UnityEngine.Sprite), //Sprite
            typeof(UnityEngine.AnimationClip), //AnimClip
        };

        public static System.Type ExtAssetPathType2UnityType(this EAssetPathType self)
        {
            if (self < 0 || (int)self >= _UnityTypes.Length)
            {
                ResManagement.ResLog._.E("unkown type {0}", self);
                return _UnityTypes[0];
            }
            return _UnityTypes[(int)self];
        }
    }
}
