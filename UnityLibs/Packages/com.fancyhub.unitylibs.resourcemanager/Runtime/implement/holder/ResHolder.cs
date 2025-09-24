using System;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18 
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH.ResManagement
{
    //UI 的资源 Holder，用来创建/加载 资源 gameobject
    internal sealed class ResHolder : CPoolItemBase, IResHolder
    {
        public IAssetHolder _asset_holder;
        public IInstHolder _inst_holder;
        public IPreInstHolder _pre_inst_holder;

        #region IAssetHolder

        public UnityEngine.Object Load(string path, EAssetPathType pathType = EAssetPathType.Default)
        {
            return _asset_holder.Load(path, pathType);
        }

        public void PreLoad(string path, EAssetPathType pathType = EAssetPathType.Default, int priority = 0)
        {
            _asset_holder.PreLoad(path, pathType, priority);
        }

        public void GetAllAsset(List<ResRef> out_list)
        {
            _asset_holder.GetAllAsset(out_list);
        }

        public HolderStat GetAssetStat()
        {
            return _asset_holder.GetAssetStat();
        }
        #endregion

        #region Inst       

        public GameObject Create(string path)
        {
            return _inst_holder.Create(path);
        }

        public void PreCreate(string path, int count, int priority = 0)
        {
            _asset_holder.PreLoad(path, EAssetPathType.Default, priority);
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
            _asset_holder.PreLoad(path, EAssetPathType.Default);
            _pre_inst_holder.PreInst(path, count);
        }
        public void ClearPreInst()
        {
            _pre_inst_holder.ClearPreInst();
        }

        #endregion

        public void SetCallBack(IHolderCallBack callback)
        {
            _asset_holder?.SetCallBack(callback);
            _inst_holder?.SetCallBack(callback);
        }
        public HolderStat GetStat()
        {
            HolderStat ret = new HolderStat();
            ret.Add(_asset_holder.GetAssetStat());
            ret.Add(_inst_holder.GetInstStat());
            return ret;
        }

        public void GetAll(List<ResRef> out_list)
        {
            _asset_holder.GetAllAsset(out_list);
            _inst_holder.GetAllInst(out_list);
        }

        protected override void OnPoolRelease()
        {
            _asset_holder.Destroy();
            _inst_holder.Destroy();
            _pre_inst_holder.Destroy();

            _pre_inst_holder = null;
            _asset_holder = null;
            _inst_holder = null;
        }
    }
}
