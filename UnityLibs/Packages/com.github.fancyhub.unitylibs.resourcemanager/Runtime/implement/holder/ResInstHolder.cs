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
        public IPreInstHolder _pre_inst_holder;
        public IResHolder _res_holder;
        public IInstHolder _inst_holder;

        public void ClearPreInst()
        {
            _pre_inst_holder.ClearPreInst();
        }

        public GameObject Create(string path)
        {
            return _inst_holder.Create(path);
        }

        public GameObject CreateEmpty()
        {
            return _inst_holder.CreateEmpty();
        }

        public void GetAll(List<ResRef> out_list)
        {
            _res_holder.GetAllRes(out_list);
            _inst_holder.GetAllInst(out_list);
        }

        public void GetAllInst(List<ResRef> out_list)
        {
            _inst_holder.GetAllInst(out_list);
        }

        public void GetAllRes(List<ResRef> out_list)
        {
            _res_holder.GetAllRes(out_list);
        }

        public HolderStat GetStat()
        {
            return _res_holder.GetStat();
        }

        public UnityEngine.Object Load(string path,bool sprite)
        {
            return _res_holder.Load(path,sprite);
        }

        public void PreInst(string path, int count)
        {
            _res_holder.PreLoad(path,false);
            _pre_inst_holder.PreInst(path, count);
        }

        public void PreLoad(string path, bool sprite, int priority = 0)
        {
            _res_holder.PreLoad(path, sprite,priority);
        }

        public bool Release(GameObject obj)
        {
            if (obj == null)
                return false;
            if (_inst_holder.Release(obj))
                return true;       

            return false;
        }

        public void ReleaseAll()
        {
            _inst_holder.ReleaseAll();
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
