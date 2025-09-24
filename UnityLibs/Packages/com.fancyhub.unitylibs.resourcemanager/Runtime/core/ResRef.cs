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
    public interface IResourcePool : ICPtr
    {
        public EResError AddUser(ResId id, System.Object user);
        public EResError RemoveUser(ResId id, System.Object user);

        public UnityEngine.Object Get(ResId id);
        public T Get<T>(ResId id) where T : UnityEngine.Object;

        public EResError TransferUser(ResId id, System.Object old_user, System.Object new_user);
    }

    public readonly struct ResRef : IEqualityComparer<ResRef>, IEquatable<ResRef>
    {
        public readonly static IEqualityComparer<ResRef> EqualityComparer = new ResRef();

        public readonly ResId Id;
        public readonly string Path;
        private readonly CPtr<IResourcePool> ResourcePool;

        internal ResRef(ResId id, string path, IResourcePool pool) { this.Id = id; this.Path = path; this.ResourcePool = new CPtr<IResourcePool>(pool); }

        /// <summary>
        /// 只是判断自己是否合法, 不判断 后面的 资源是否已经被释放
        /// </summary>
        public bool IsValid()
        {
            if (ResourcePool.Null)
                return false;
            return Id.IsValid();
        }

        public UnityEngine.Object Get() { return ResourcePool.Val?.Get(Id); }
        public T Get<T>() where T : UnityEngine.Object { return ResourcePool.Val?.Get<T>(Id); }
        public bool AddUser(System.Object user)
        {
            if (ResourcePool.Val == null || !Id.IsValid())
                return false;
            if (ResourcePool.Val.AddUser(Id, user) == EResError.OK)
                return true;
            return false;
        }
        public bool RemoveUser(System.Object user)
        {
            if (ResourcePool.Val == null || !Id.IsValid())
                return false;
            if (ResourcePool.Val.RemoveUser(Id, user) == EResError.OK)
                return true;
            return false;
        }

        public override string ToString()
        {
            return $"ResId:({Id.Id},{Id.ResType}) AssetPath:({Path})";
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
