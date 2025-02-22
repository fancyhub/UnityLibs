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
    public interface IResPool : ICPtr
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
        private readonly CPtr<IResPool> ResPool;

        internal ResRef(ResId id, string path, IResPool pool) { this.Id = id; this.Path = path; this.ResPool = new CPtr<IResPool>(pool); }

        /// <summary>
        /// 只是判断自己是否合法, 不判断 后面的 资源是否已经被释放
        /// </summary>
        public bool IsValid()
        {
            if (ResPool.Null)
                return false;
            return Id.IsValid();
        }

        public UnityEngine.Object Get() { return ResPool.Val?.Get(Id); }
        public T Get<T>() where T : UnityEngine.Object { return ResPool.Val?.Get<T>(Id); }
        public void AddUser(System.Object user) { if (Id.IsValid()) ResPool.Val?.AddUser(Id, user); }
        public void RemoveUser(System.Object user) { if (Id.IsValid()) ResPool.Val?.RemoveUser(Id, user); }

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
