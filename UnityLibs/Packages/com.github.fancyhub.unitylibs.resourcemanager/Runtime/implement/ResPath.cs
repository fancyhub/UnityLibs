/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.ResManagement
{
    internal readonly struct ResPath : IEquatable<ResPath>, IEqualityComparer<ResPath>
    {
        public static IEqualityComparer<ResPath> EqualityComparer = new ResPath();

        public static ResPath Empty = new ResPath(string.Empty, EResPathType.Default);
        public readonly string Path;
        public readonly EResPathType PathType;

        internal static ResPath Create(string path, EResPathType pathType) { return new ResPath(path, pathType); }
        internal static ResPath CreateRes(string path) { return new ResPath(path, EResPathType.Default); }
        internal static ResPath CreateSprite(string path) { return new ResPath(path, EResPathType.Sprite); }

        private ResPath(string path, EResPathType path_type) { Path = path; PathType = path_type; }

        public bool Equals(ResPath other) { return Path == other.Path && PathType == other.PathType; }

        public bool IsValid() { return !string.IsNullOrEmpty(Path); }

        public override int GetHashCode() { return System.HashCode.Combine(Path, PathType); }

        public override bool Equals(object obj)
        {
            if (obj is ResPath other) return Path == other.Path && PathType == other.PathType;
            return false;
        }

        public override string ToString() { return $"{Path}:{PathType}"; }


        public static bool operator ==(ResPath left, ResPath right) { return left.Path == right.Path && left.PathType == right.PathType; }
        public static bool operator !=(ResPath left, ResPath right) { return left.Path != right.Path || left.PathType != right.PathType; }


        #region IEqualityComparer<ResPath>
        bool IEqualityComparer<ResPath>.Equals(ResPath x, ResPath y) { return x.Path == y.Path && x.PathType == y.PathType; }

        int IEqualityComparer<ResPath>.GetHashCode(ResPath obj) { return System.HashCode.Combine(obj.Path, obj.PathType); }
        #endregion
    }
}
