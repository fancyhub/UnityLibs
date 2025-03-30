/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using FH.ResManagement;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
namespace FH
{
    using ResAwaitSource = AwaitableCompletionSource<(EResError error, ResRef res_ref)>;

    public delegate void ResEvent(int job_id, EResError error, ResRef res_ref);
    public delegate void InstEvent(int job_id, EResError error, ResRef res_ref);
    public interface IResDoneCallBack : ICPtr
    {
        public void OnResDoneCallback(int job_id, EResError error, ResRef res_ref);
    }

    public interface IInstDoneCallBack : ICPtr
    {
        public void OnResDoneCallback(int job_id, EResError error, ResRef res_ref);
    }

    public struct ResSnapShotItem
    {
        public string Path;
        public BitEnum32<EResPathType> PathType;
        public ResId Id;
        public int UserCount;
        public List<System.Object> Users; //Editor 模式下才有内容
    }

    public enum EAssetStatus
    {
        Exist,
        NotExist,
        NotDownloaded,
    }


    public partial interface IResMgr : ICPtr
    {
        public void Update();

        public EAssetStatus GetAssetStatus(string path);

        #region Res
        public EResError Load(string path, EResPathType pathType, bool only_from_cache, out ResRef res_ref);
        public EResError LoadAsync(string path, EResPathType pathType, int priority, ResEvent cb, out int job_id);
        public EResError LoadAsync(string path, EResPathType pathType, int priority, IResDoneCallBack cb, out int job_id);
        public EResError LoadAsync(string path, EResPathType pathType, int priority, ResAwaitSource source, CancellationToken cancelToken);
        public void ResSnapshot(ref List<ResSnapShotItem> out_snapshot);
        #endregion

        #region GameObject Inst
        public EResError Create(string path, System.Object user, bool only_from_cache, out ResRef res_ref);
        public EResError CreateAsync(string path, int priority, InstEvent cb, out int job_id);
        public EResError CreateAsync(string path, int priority, IInstDoneCallBack cb, out int job_id);
        public EResError CreateAsync(string path, int priority, ResAwaitSource source, CancellationToken cancelToken);
        #endregion

        #region 预实例化
        public EResError ReqPreInst(string path, int count, out int req_id);
        public EResError CancelPreInst(int req_id);
        #endregion  

        public void CancelJob(int job_id);

        public ResRef GetResRef(UnityEngine.Object res);
    }

    public static class ResMgr
    {
        private static CPtr<IResMgr> _;

        public static bool InitMgr(IResMgr.Config config, IResMgr.IExternalLoader external_loader)
        {
            if (!_.Null)
            {
                ResManagement.ResLog._.Assert(false, "ResMgr 已经存在了");
                return false;
            }

            if (external_loader == null)
            {
                ResManagement.ResLog._.Assert(false, "AssetLoader Is null");
                return false;
            }

            if (config == null)
            {
                ResManagement.ResLog._.Assert(false, "ResMgrConfig Is null");
                return false;
            }

            ResLog._ = TagLog.Create(ResLog._.Tag, config.LogLevel);
            ResManagement.ResMgrImplement mgr = new ResManagement.ResMgrImplement(external_loader, config);
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
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return EAssetStatus.NotExist;
            }
            return inst.GetAssetStatus(path);
        }

        /// <summary>
        /// 创建一个Holder
        /// </summary>
        /// <param name="only_from_cache">是否只从缓存里面加载</param>
        /// <param name="share_inst">实例对象是否在多个Holder之间共享</param>
        /// <returns></returns>
        public static IResInstHolder CreateHolder(bool only_from_cache, bool share_inst)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return null;
            }

            ResManagement.ResInstHolder ret = GPool.New<ResManagement.ResInstHolder>();
            ret._res_holder = ResManagement.ResHolder.Create(inst, only_from_cache);
            ret._inst_holder = ResManagement.InstHolder.Create(inst, only_from_cache, share_inst);
            ret._pre_inst_holder = ResManagement.PreInstHolder.Create(inst);
            return ret;
        }

        #region Res
        #region Sprite
        public static ResRef TryLoadExistSprite(string path)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }

            var err = inst.Load(path, EResPathType.Sprite, false, out var res_ref);
            return res_ref;
        }

        public static ResRef LoadSprite(string path, bool only_from_cache = false)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }

            var err = inst.Load(path, EResPathType.Sprite, only_from_cache, out var res_ref);
            ResManagement.ResLog._.ErrCode(err, path);
            return res_ref;
        }
        #endregion

        #region Default
        public static ResRef TryLoadExist(string path, EResPathType pathType = EResPathType.Default)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }

            var err = inst.Load(path, pathType, false, out var res_ref);
            return res_ref;
        }

        public static ResRef Load(string path, EResPathType pathType = EResPathType.Default, bool only_from_cache = false)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }

            var err = inst.Load(path, pathType, only_from_cache, out var res_ref);
            ResManagement.ResLog._.ErrCode(err, path);
            return res_ref;
        }

        public static bool AsyncLoad(string path, EResPathType pathType, ResEvent cb, out int job_id, int priority = ResDef.PriorityDefault)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                job_id = default;
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return false;
            }

            var err = inst.LoadAsync(path, pathType, priority, cb, out job_id);
            ResManagement.ResLog._.ErrCode(err, path);
            return err == EResError.OK;
        }

        public static bool AsyncLoad(string path, EResPathType pathType, IResDoneCallBack cb, out int job_id, int priority = ResDef.PriorityDefault)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                job_id = default;
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return false;
            }

            var err = inst.LoadAsync(path, pathType, priority, cb, out job_id);
            ResManagement.ResLog._.ErrCode(err, path);
            return err == EResError.OK;
        }

        public static async Awaitable<ResRef> AsyncLoad(string path, EResPathType pathType, CancellationToken cancelToken, int priority = ResDef.PriorityDefault)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default(ResRef);
            }

            AwaitableCompletionSource<(EResError error, ResRef res_ref)> source = new AwaitableCompletionSource<(EResError error, ResRef res_ref)>();

            var err = inst.LoadAsync(path, pathType, priority, source, cancelToken);
            ResManagement.ResLog._.ErrCode(err, path);
            if (err != EResError.OK)
                return default(ResRef);
            var (err1, res_ref) = await source.Awaitable;
            ResManagement.ResLog._.ErrCode(err1, path);
            return res_ref;
        }

        public static void ResSnapshot(ref List<ResSnapShotItem> out_snapshot)
        {
            var inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return;
            }

            inst.ResSnapshot(ref out_snapshot);
        }
        #endregion
        #endregion

        #region GameObject Inst
        public static ResRef Create(string path, System.Object user, bool only_from_cache)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }
            var err = inst.Create(path, user, only_from_cache, out var res_ref);
            ResManagement.ResLog._.ErrCode(err, path);
            return res_ref;
        }
        public static bool AsyncCreate(string path, InstEvent cb, out int job_id, int priority = ResDef.PriorityDefault)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                job_id = default;
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return false;
            }
            var err = inst.CreateAsync(path, priority, cb, out job_id);
            ResManagement.ResLog._.ErrCode(err, path);
            return err == EResError.OK;
        }

        public static bool AsyncCreate(string path, IInstDoneCallBack cb, out int job_id, int priority = ResDef.PriorityDefault)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                job_id = default;
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return false;
            }
            var err = inst.CreateAsync(path, priority, cb, out job_id);
            ResManagement.ResLog._.ErrCode(err, path);
            return err == EResError.OK;
        }

        public static async Awaitable<ResRef> AsyncCreate(string path, CancellationToken cancelToken = default, int priority = ResDef.PriorityDefault)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default(ResRef);
            }

            ResAwaitSource source = new ResAwaitSource();
            var err = inst.CreateAsync(path, priority, source, cancelToken);
            ResManagement.ResLog._.ErrCode(err, path);
            if (err != EResError.OK)
                return default(ResRef);

            var (err1, res_ref) = await source.Awaitable;
            ResManagement.ResLog._.ErrCode(err, path);
            if (err1 != EResError.OK)
                return default(ResRef);
            return res_ref;
        }
        #endregion

        #region 预实例化
        public static bool ReqPreInst(string path, int count, out int req_id)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                req_id = default;
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return false;
            }
            var err = inst.ReqPreInst(path, count, out req_id);
            ResManagement.ResLog._.ErrCode(err, path);
            return err == EResError.OK;
        }
        public static bool CancelPreInst(int req_id)
        {
            IResMgr inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return false;
            }
            var err = inst.CancelPreInst(req_id);
            ResManagement.ResLog._.ErrCode(err);
            return err == EResError.OK;
        }
        #endregion;        

        public static void CancelJob(int job_id)
        {
            var inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return;
            }
            inst.CancelJob(job_id);
        }

        public static ResRef GetResRef(UnityEngine.Object res)
        {
            var inst = _.Val;
            if (inst == null)
            {
                ResManagement.ResLog._.ErrCode(EResError.ResMgrNotInit);
                return default;
            }
            return inst.GetResRef(res);
        }
    }
}
