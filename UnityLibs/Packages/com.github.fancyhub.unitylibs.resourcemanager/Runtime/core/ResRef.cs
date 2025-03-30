/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace FH
{
    public interface IResInstPool : ICPtr
    {
        EResError AddUser(ResId id, System.Object user);
        EResError RemoveUser(ResId id, System.Object user);

        UnityEngine.Object Get(ResId id);
        T Get<T>(ResId id) where T : UnityEngine.Object;
    }

    public readonly struct ResRef : IEqualityComparer<ResRef>, IEquatable<ResRef>
    {
        public readonly static IEqualityComparer<ResRef> EqualityComparer = new ResRef();

        public readonly ResId Id;
        public readonly string Path;
        private readonly CPtr<IResInstPool> ResInstPool;

        internal ResRef(ResId id, string path, IResInstPool pool) { this.Id = id; this.Path = path; this.ResInstPool = new CPtr<IResInstPool>(pool); }

        /// <summary>
        /// 只是判断自己是否合法, 不判断 后面的 资源是否已经被释放
        /// </summary>
        public bool IsValid()
        {
            if (ResInstPool.Null)
                return false;
            return Id.IsValid();
        }

        public UnityEngine.Object Get() { return ResInstPool.Val?.Get(Id); }
        public T Get<T>() where T : UnityEngine.Object { return ResInstPool.Val?.Get<T>(Id); }
        public bool AddUser(System.Object user)
        { 
            if (ResInstPool.Val==null || !Id.IsValid()) 
                return false;
            if (ResInstPool.Val.AddUser(Id, user) == EResError.OK)
                return true;
            return false;
        }
        public bool RemoveUser(System.Object user) 
        {
            if (ResInstPool.Val == null || !Id.IsValid())
                return false;
            if (ResInstPool.Val.RemoveUser(Id, user) == EResError.OK)
                return true;
            return false;
        }

        public override string ToString()
        {
            return $"ResId:({Id.Id},{Id.ResType}) ResPath:({Path})";
        }

        bool IEqualityComparer<ResRef>.Equals(ResRef x, ResRef y)
        {
            return x.Id.Equals(y.Id);
        }

        int IEqualityComparer<ResRef>.GetHashCode(ResRef obj)
        {
            return obj.Id.GetHashCode();
        }

        public bool Equals(ResRef other) { return other.Id.Equals(Id); }
        public override int GetHashCode() { return Id.GetHashCode(); }
    }
}
