/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
 

namespace FH
{
    public readonly struct ResPath : IEquatable<ResPath>
    {
        public readonly string Path;
        public readonly bool Sprite;

        public ResPath(string path, bool sprite) { Path = path; Sprite = sprite; }

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
    }
     
}
