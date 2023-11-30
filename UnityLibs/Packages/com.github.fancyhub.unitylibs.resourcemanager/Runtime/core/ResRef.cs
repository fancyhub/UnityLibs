/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

namespace FH
{
    public interface IResPool : ICPtr
    {
        EResError AddUser(ResId id, System.Object user);
        EResError RemoveUser(ResId id, System.Object user);

        UnityEngine.Object Get(ResId id);
        T Get<T>(ResId id) where T : UnityEngine.Object;
    }

    public readonly struct ResRef
    {
        public readonly ResId Id;
        public readonly ResPath Path;
        private readonly CPtr<IResPool> ResPool;

        public ResRef(ResId id, ResPath path, IResPool pool) { this.Id = id; this.Path = path; this.ResPool = new CPtr<IResPool>(pool); }

        public bool IsSelfValid()
        {
            if (ResPool.Null)
                return false;
            return Id.IsValid();
        }

        public UnityEngine.Object Get() { return ResPool.Val?.Get(Id); }
        public T Get<T>() where T : UnityEngine.Object { return ResPool.Val?.Get<T>(Id); }
        public void AddUser(System.Object user) { ResPool.Val?.AddUser(Id, user); }
        public void RemoveUser(System.Object user) { ResPool.Val?.RemoveUser(Id, user); }

        public override string ToString()
        {
            return $"ResId:({Id.Id},{Id.ResType}) ResPath:({Path.Path},{Path.Sprite})";
        }
    }     
}
