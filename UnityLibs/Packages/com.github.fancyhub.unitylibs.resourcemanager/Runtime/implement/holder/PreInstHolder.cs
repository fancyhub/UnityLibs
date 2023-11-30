using System;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/3 14:44:07
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH.Res
{
    internal sealed class PreInstHolder : CPoolItemBase, IPreInstHolder
    {
        public HashSet<int> _preload_set = new HashSet<int>();
        private CPtr<IResMgr> _ResMgr;
        internal static PreInstHolder Create(IResMgr res_mgr)
        {
            var ret = GPool.New<PreInstHolder>();
            ret._ResMgr = new CPtr<IResMgr>(res_mgr);
            return ret;
        }

        public void PreInst(string path, int count)
        {
            IResMgr res_mgr = _ResMgr.Val;
            if (res_mgr == null)
                return;

            EResError err = res_mgr.ReqPreInst(path, count, out int req_id);
            if (err == EResError.OK)
                _preload_set.Add(req_id);
        }

        public void ClearPreInst()
        {
            IResMgr res_mgr = _ResMgr.Val;
            if (res_mgr == null)
            {
                _preload_set.Clear();
                return;
            }

            foreach (var id in _preload_set)
            {
                res_mgr.CancelPreInst(id);
            }

            _preload_set.Clear();
        }

        protected override void OnPoolRelease()
        {
            ClearPreInst();
        }
    }
}
