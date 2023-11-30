using System;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/3 14:52:27
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH.Res
{
    internal sealed class EmptyInstHolder : CPoolItemBase, IEmptyInstHolder
    {
        private CPtr<IResMgr> _ResMgr;
        public MyDict<int, ResRef> _Dict = new MyDict<int, ResRef>();
        internal static EmptyInstHolder Create(IResMgr res_mgr)
        {
            var ret= GPool.New<EmptyInstHolder>();
            ret._ResMgr = new CPtr<IResMgr>(res_mgr);
            return ret;
        }

        public GameObject CreateEmpty()
        {
            IResMgr res_mgr = _ResMgr.Val;
            if (res_mgr == null)
                return null;

            EResError err = res_mgr.CreateEmpty(this, out var res_ref);
            if (err != EResError.OK)
                return null;
            GameObject obj = res_ref.Get<GameObject>();
            if (obj == null)
                return null;

            _Dict.Add(obj.GetInstanceID(), res_ref);
            return obj;
        }

        public bool Release(GameObject obj)
        {
            if (obj == null)
                return false;
            int inst_id = obj.GetInstanceID();
            if (!_Dict.Remove(inst_id, out var res_ref))
                return false;

            res_ref.RemoveUser(this);
            return true;
        }

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
