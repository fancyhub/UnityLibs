/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
   
    //public struct SceneID : IEquatable<SceneID>, IEqualityComparer<SceneID>
    //{
    //    public static IEqualityComparer<SceneID> EqualityComparer = new SceneID();

    //    public static readonly SceneID Empty = new SceneID(0);
    //    private static int _IdGen = 0;
    //    public readonly int Id;
    //    public static SceneID Create() { _IdGen++; return new SceneID(_IdGen); }

    //    public bool Equals(SceneID other) { return other.Id == Id; }
    //    private SceneID(int id) { this.Id = id; }

    //    bool IEqualityComparer<SceneID>.Equals(SceneID x, SceneID y) { return x.Id == y.Id; }
    //    int IEqualityComparer<SceneID>.GetHashCode(SceneID obj) { return obj.Id; }

    //    public override int GetHashCode() { return Id; }

    //    public override string ToString()
    //    {
    //        return Id.ToString();
    //    }
    //}
}
