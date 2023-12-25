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
        public static ResPath Empty = new ResPath(string.Empty, false);
        public readonly string Path;
        public readonly bool Sprite;

        internal static ResPath Create(string path,bool sprite) { return new ResPath(path, sprite); }
        internal static ResPath CreateRes(string path) { return new ResPath(path, false); }
        internal static ResPath CreateSprite(string path) { return new ResPath(path, true); }

        private ResPath(string path, bool sprite) { Path = path; Sprite = sprite; }

        public bool Equals(ResPath other) { return Path == other.Path && Sprite == other.Sprite; }

        public bool IsValid() { return !string.IsNullOrEmpty(Path); }

        public override int GetHashCode() { return System.HashCode.Combine(Path, Sprite); }

        public override bool Equals(object obj)
        {
            if (obj is ResPath other) return Path == other.Path && Sprite == other.Sprite;
            return false;
        }

        public override string ToString() { return Path; }


        public static bool operator ==(ResPath left, ResPath right) { return left.Path == right.Path && left.Sprite == right.Sprite; }
        public static bool operator !=(ResPath left, ResPath right) { return left.Path != right.Path || left.Sprite != right.Sprite; }


        #region IEqualityComparer<ResPath>
        bool IEqualityComparer<ResPath>.Equals(ResPath x, ResPath y) { return x.Path == y.Path && x.Sprite == y.Sprite; }

        int IEqualityComparer<ResPath>.GetHashCode(ResPath obj) { return System.HashCode.Combine(obj.Path, obj.Sprite); }        
        #endregion
    }
}
