/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using FH.Res;
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

        public static bool InitMgr(IAssetLoader asset_loader, ResMgrConfig conf)
        {
            if (!_.Null)
            {
                Res.ResLog._.Assert(false, "ResMgr 已经存在了");
                return false;
            }

            ResLog._ = TagLogger.Create(ResLog._.Tag, conf.LogLevel);
            Res.ResMgrImplement mgr = new Res.ResMgrImplement();
            mgr.Init(asset_loader, conf);
            _ = mgr;
            return true;
        }

        public static void Update()
        {
            _.Val?.Update();
        }

        public static void Destroy()
        {
            _.Destroy();
        }

        public static EAssetStatus GetAssetStatus(string path)
        {
            var inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return EAssetStatus.NotExist;
            }
            return inst.GetAssetStatus(path);
        }

        /// <summary>
        /// 创建一个Holder
        /// </summary>
        /// <param name="sync_load_enable">是否允许同步加载</param>
        /// <param name="share_inst">实例对象是否在多个Holder之间共享</param>
        /// <returns></returns>
        public static IResInstHolder CreateHolder(bool sync_load_enable, bool share_inst)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return null;
            }

            Res.ResInstHolder ret = GPool.New<Res.ResInstHolder>();
            ret._res_holder = Res.ResHolder.Create(inst, sync_load_enable);
            ret._inst_holder = Res.InstHolder.Create(inst, sync_load_enable, share_inst);
            ret._pre_inst_holder = Res.PreInstHolder.Create(inst);
            return ret;
        }

        #region Res

        public static ResRef LoadSprite(string path, bool aync_load_enable = true)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }

            var err = inst.Load(path, true, aync_load_enable, out var res_ref);
            Res.ResLog._.ErrCode(err, path);
            return res_ref;
        }

        public static ResRef Load(string path, bool aync_load_enable = true)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }

            var err = inst.Load(path, false, aync_load_enable, out var res_ref);
            Res.ResLog._.ErrCode(err, path);
            return res_ref;
        }

        public static EResError AsyncLoad(string path, bool sprite, int priority, ResEvent cb, out int job_id)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                job_id = default;
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return EResError.ResMgrNotInit;
            }

            var err = inst.AsyncLoad(path, sprite, priority, cb, out job_id);
            Res.ResLog._.ErrCode(err, path);
            return err;
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
        public static ResRef Create(string path, System.Object user, bool aync_load_enable)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }
            var err = inst.Create(path, user, aync_load_enable, out var res_ref);
            Res.ResLog._.ErrCode(err, path);
            return res_ref;
        }
        public static EResError AsyncCreate(string path, int priority, ResEvent cb, out int job_id)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                job_id = default;
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return EResError.ResMgrNotInit;
            }
            var err = inst.AsyncCreate(path, priority, cb, out job_id);
            Res.ResLog._.ErrCode(err, path);
            return err;
        }
        #endregion

        #region 预实例化
        public static EResError ReqPreInst(string path, int count, out int req_id)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                req_id = default;
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return EResError.ResMgrNotInit;
            }
            var err = inst.ReqPreInst(path, count, out req_id);
            Res.ResLog._.ErrCode(err, path);
            return err;
        }
        public static EResError CancelPreInst(int req_id)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return EResError.ResMgrNotInit;
            }
            var err = inst.CancelPreInst(req_id);
            Res.ResLog._.ErrCode(err);
            return err;
        }
        #endregion;

        #region Empty
        public static ResRef CreateEmpty(System.Object user)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                Res.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }
            var err = inst.CreateEmpty(user, out var res_ref);
            Res.ResLog._.ErrCode(err);
            return res_ref;
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
