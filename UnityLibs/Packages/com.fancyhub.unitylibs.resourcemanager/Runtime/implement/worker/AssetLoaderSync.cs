/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;

namespace FH.ResManagement
{
    //同步创建流程
    internal class AssetLoaderSync : IMsgProc<ResJob>
    {
        #region 传入
        public AssetPool _asset_pool;
        public IResMgr.IExternalAssetLoader _external_loader;
        public ResMsgQueue _msg_queue;
        #endregion


        public void Init()
        {
            _msg_queue.Reg(EResWoker.sync_load_asset, this);
        }

        public void Destroy()
        {
            _msg_queue.UnReg(EResWoker.sync_load_asset);
        }

        public void OnMsgProc(ref ResJob job)
        {
            //2. 如果任务取消了，就从db里面移除            
            if (job.IsCancelled)
            {
                _msg_queue.SendJobNext(job);
                return;
            }

            //3.判断一定是自己
            EResWoker job_type = job.GetCurrentWorker();
            if (job_type != EResWoker.sync_load_asset)
            {
                ResLog._.Assert(false, "job 不是 类型 {0}!={1}, {2}", job_type, EResWoker.sync_load_asset, job.Path.Path);
                _msg_queue.SendJobNext(job);
                return;
            }

            //4. 先查找是否存在
            EResError err = _asset_pool.GetIdByPath(job.Path, out job.AssetId);
            if (err == EResError.OK)
            {
                _msg_queue.SendJobNext(job);
                return;
            }

            //5. 不存在, 加载
            IResMgr.IExternalAssetRef asset_ref = _external_loader.Load(job.Path.Path, job.Path.PathType.ExtAssetPathType2UnityType());
            if (asset_ref == null)
            {
                ResLog._.D("load failed res {0}", job.Path);
                job.ErrorCode = EResError.AssetLoaderSync_load_asset_failed;
                _msg_queue.SendJobNext(job);
                return;
            }
            else//添加到 ResPool
            {
                ResLog._.D("load res  {0}", job.Path);
                err = _asset_pool.AddAsset(job.Path, asset_ref, out job.AssetId);
                ResLog._.ErrCode(err);
                if (err != EResError.OK)
                {
                    asset_ref.Destroy();
                }
            }

            //直接发到下一个
            _msg_queue.SendJobNext(job);
        }
    }
}
