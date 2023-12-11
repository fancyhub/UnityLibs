/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;

namespace FH
{
    public delegate void ResEvent(EResError code, string path, EResType resType, int job_id);

    public struct ResSnapShotItem
    {
        public string Path;
        public bool Sprite;
        public ResId Id;
        public int UserCount;
        public List<object> Users; //Editor 模式下才有内容
    }


    internal interface IResMgr : ICPtr
    {
        public void Update();

        public EAssetStatus GetAssetStatus(string path);

        #region Res
        public EResError Load(string path, bool sprite, bool aync_load_enable, out ResRef res_ref);
        public EResError AsyncLoad(string path, bool sprite, int priority, ResEvent cb, out int job_id);
        public void ResSnapshot(ref List<ResSnapShotItem> out_snapshot);
        #endregion

        #region GameObject Inst
        public EResError Create(string path, System.Object user, bool aync_load_enable, out ResRef res_ref);
        public EResError AsyncCreate(string path, int priority, ResEvent cb, out int job_id);
        #endregion

        #region 预实例化
        public EResError ReqPreInst(string path, int count, out int req_id);
        public EResError CancelPreInst(int req_id);
        #endregion;

        #region Empty
        public EResError CreateEmpty(System.Object user, out ResRef res_ref);
        #endregion

        public void CancelJob(int job_id);
    }

    public static class ResMgr
    {
        private static CPtr<IResMgr> _;

        public static void Destroy()
        {
            _.Destroy();
        }

        public static IResInstHolder CreateHolder(bool sync_load_enable)
        {
            IResMgr mgr = _.Val;
            if (mgr == null)
                return null;

            Res.ResInstHolder ret = GPool.New<Res.ResInstHolder>();
            ret._res_holder = Res.ResHolder.Create(mgr, sync_load_enable);
            ret._inst_holder = Res.InstHolder.Create(mgr, sync_load_enable);
            ret._pre_inst_holder = Res.PreInstHolder.Create(mgr);
            return ret;
        }

        public static bool InitMgr(IAssetLoader asset_loader, ResMgrConfig conf)
        {
            if (!_.Null)
                return false;

            Res.ResMgrImplement mgr = new Res.ResMgrImplement();
            mgr.Init(asset_loader, conf);
            _ = mgr;
            return true;
        }

        public static void Update()
        {
            _.Val?.Update();
        }

        public static EAssetStatus GetAssetStatus(string path)
        {
            var inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return  EAssetStatus.NotExist;
            }
            return inst.GetAssetStatus(path);
        }

        #region Res
        public static EResError Load(string path, bool sprite, bool aync_load_enable, out ResRef res_ref)
        {
            res_ref = default;
            var inst = _.Val;
            if (inst == null) return EResError.ResMgrNotInit;
            return inst.Load(path, sprite, aync_load_enable, out res_ref);
        }

        public static EResError AsyncLoad(string path, bool sprite, int priority, ResEvent cb, out int job_id)
        {
            job_id = default;
            var inst = _.Val;
            if (inst == null) return EResError.ResMgrNotInit;
            return inst.AsyncLoad(path, sprite, priority, cb, out job_id);
        }

        public static void ResSnapshot(ref List<ResSnapShotItem> out_snapshot)
        {
            var inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return;
            }

            inst.ResSnapshot(ref out_snapshot);
        }
        #endregion

        #region GameObject Inst
        public static EResError Create(string path, System.Object user, bool aync_load_enable, out ResRef res_ref)
        {
            res_ref = default;
            var inst = _.Val;
            if (inst == null) return EResError.ResMgrNotInit;
            return inst.Create(path, user, aync_load_enable, out res_ref);
        }
        public static EResError AsyncCreate(string path, int priority, ResEvent cb, out int job_id)
        {
            job_id = default;
            var inst = _.Val;
            if (inst == null) return EResError.ResMgrNotInit;
            return inst.AsyncCreate(path, priority, cb, out job_id);
        }
        #endregion

        #region 预实例化
        public static EResError ReqPreInst(string path, int count, out int req_id)
        {
            req_id = default;
            var inst = _.Val;
            if (inst == null) return EResError.ResMgrNotInit;
            return inst.ReqPreInst(path, count, out req_id);
        }
        public static EResError CancelPreInst(int req_id)
        {
            var inst = _.Val;
            if (inst == null) return EResError.ResMgrNotInit;
            return inst.CancelPreInst(req_id);
        }
        #endregion;

        #region Empty
        public static EResError CreateEmpty(System.Object user, out ResRef res_ref)
        {
            res_ref = default;
            var inst = _.Val;
            if (inst == null) return EResError.ResMgrNotInit;
            return inst.CreateEmpty(user, out res_ref);
        }
        #endregion

        public static void CancelJob(int job_id)
        {
            var inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return;
            }
            inst.CancelJob(job_id);
        }
    }
}
