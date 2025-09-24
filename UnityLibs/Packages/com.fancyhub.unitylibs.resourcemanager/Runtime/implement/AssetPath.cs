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
    internal readonly struct AssetPath : IEquatable<AssetPath>, IEqualityComparer<AssetPath>
    {
        public static IEqualityComparer<AssetPath> EqualityComparer = new AssetPath();

        public static AssetPath Empty = new AssetPath(string.Empty, EAssetPathType.Default);
        public readonly string Path;
        public readonly EAssetPathType PathType;

        internal static AssetPath Create(string path, EAssetPathType pathType= EAssetPathType.Default) { return new AssetPath(path, pathType); }
        internal static AssetPath CreateSprite(string path) { return new AssetPath(path, EAssetPathType.Sprite); }

        private AssetPath(string path, EAssetPathType path_type) { Path = path; PathType = path_type; }

        public bool Equals(AssetPath other) { return Path == other.Path && PathType == other.PathType; }

        public bool IsValid() { return !string.IsNullOrEmpty(Path); }

        public override int GetHashCode() { return System.HashCode.Combine(Path, PathType); }

        public override bool Equals(object obj)
        {
            if (obj is AssetPath other) return Path == other.Path && PathType == other.PathType;
            return false;
        }

        public override string ToString() { return $"{Path}:{PathType}"; }


        public static bool operator ==(AssetPath left, AssetPath right) { return left.Path == right.Path && left.PathType == right.PathType; }
        public static bool operator !=(AssetPath left, AssetPath right) { return left.Path != right.Path || left.PathType != right.PathType; }


        #region IEqualityComparer<ResPath>
        bool IEqualityComparer<AssetPath>.Equals(AssetPath x, AssetPath y) { return x.Path == y.Path && x.PathType == y.PathType; }

        int IEqualityComparer<AssetPath>.GetHashCode(AssetPath obj) { return System.HashCode.Combine(obj.Path, obj.PathType); }
        #endregion
    }

    
}
