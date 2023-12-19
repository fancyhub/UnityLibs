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
    //UI 的资源 Holder，用来创建/加载 资源 gameobject
    internal sealed class ResInstHolder : CPoolItemBase, IResInstHolder
    {
        public IResHolder _res_holder;
        public IInstHolder _inst_holder;
        public IPreInstHolder _pre_inst_holder;

        #region IResHolder

        public UnityEngine.Object Load(string path, bool sprite)
        {
            return _res_holder.Load(path, sprite);
        }

        public void PreLoad(string path, bool sprite, int priority = 0)
        {
            _res_holder.PreLoad(path, sprite, priority);
        }

        public void GetAllRes(List<ResRef> out_list)
        {
            _res_holder.GetAllRes(out_list);
        }

        public HolderStat GetResStat()
        {
            return _res_holder.GetResStat();
        }
        #endregion

        #region Inst
        public GameObject CreateEmpty()
        {
            return _inst_holder.CreateEmpty();
        }

        public GameObject Create(string path)
        {
            return _inst_holder.Create(path);
        }

        public void PreCreate(string path, int count, int priority = 0)
        {
            _res_holder.PreLoad(path, false, priority);
            _inst_holder.PreCreate(path, count, priority);
            _pre_inst_holder.PreInst(path, count);
        }

        public void GetAllInst(List<ResRef> out_list)
        {
            _inst_holder.GetAllInst(out_list);
        }

        public HolderStat GetInstStat()
        {
            return _inst_holder.GetInstStat();
        }

        public bool Release(GameObject obj)
        {
            if (obj == null)
                return false;
            if (_inst_holder.Release(obj))
                return true;

            return false;
        }
        #endregion

        #region Pre Inst
        public void PreInst(string path, int count)
        {
            _res_holder.PreLoad(path, false);
            _pre_inst_holder.PreInst(path, count);
        }
        public void ClearPreInst()
        {
            _pre_inst_holder.ClearPreInst();
        }

        #endregion


        public HolderStat GetStat()
        {
            HolderStat ret = new HolderStat();
            ret.Add(_res_holder.GetResStat());
            ret.Add(_inst_holder.GetInstStat());
            return ret;
        }

        public void GetAll(List<ResRef> out_list)
        {
            _res_holder.GetAllRes(out_list);
            _inst_holder.GetAllInst(out_list);
        }

        protected override void OnPoolRelease()
        {
            _res_holder.Destroy();
            _inst_holder.Destroy();
            _pre_inst_holder.Destroy();

            _pre_inst_holder = null;
            _res_holder = null;
            _inst_holder = null;
        }
    }
}
