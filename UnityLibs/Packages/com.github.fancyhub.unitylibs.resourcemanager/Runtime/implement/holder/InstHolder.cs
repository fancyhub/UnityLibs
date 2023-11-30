using System;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18 
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH.Res
{
    /// <summary>
    /// 非独占实例的holder，但是destroy的时候，会清除当前正在用的所有对象
    /// </summary>
    internal sealed class InstHolder : CPoolItemBase, IInstHolder
    {
        public const string C_USER = "Holder User";
        private bool _AsyncLoadEnable;
        private CPtr<IResMgr> _ResMgr;
        public MyDict<int, ResRef> _Dict = new MyDict<int, ResRef>();

        internal static InstHolder Create(IResMgr res_mgr, bool sync_load_enable)
        {
            var ret = GPool.New<InstHolder>();
            ret._AsyncLoadEnable = sync_load_enable;
            ret._ResMgr = new CPtr<IResMgr>(res_mgr);
            return ret;
        }

        public GameObject Create(string path)
        {
            IResMgr res_mgr = _ResMgr.Val;
            if (res_mgr == null)
                return null;
            EResError err = res_mgr.Create(path, this, _AsyncLoadEnable, out ResRef res_ref);
            ResLog._.ErrCode(err, $"创建实例失败 {path}");

            if (err != EResError.OK)
                return null;

            ResLog._.D("{0}", res_ref);

            GameObject obj = res_ref.Get<GameObject>();
            if (obj == null)
            {
                ResLog._.Assert(false, "Error");
                res_ref.RemoveUser(this);
                return null;
            }

            _Dict.Add(obj.GetInstanceID(), res_ref);

            return obj;
        }

        /// <summary>
        /// 回收一个实例对象
        /// </summary>        
        public bool Release(GameObject obj)
        {
            if (obj == null)
                return false;
            int inst_id = obj.GetInstanceID();

            if (!_Dict.Remove(inst_id, out ResRef res_ref))
            {
                GoUtil.Destroy(obj);
                ResLog._.Assert(false, "Error");
                return true;
            }

            res_ref.RemoveUser(this);
            return true;
        }

        /// <summary>
        /// 释放所有Holder住的PoolObject对象
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var p in _Dict)
            {
                p.Value.RemoveUser(this);
            }
            _Dict.Clear();
        }

        protected override void OnPoolRelease()
        {
            ReleaseAll();
        }
    }
}
