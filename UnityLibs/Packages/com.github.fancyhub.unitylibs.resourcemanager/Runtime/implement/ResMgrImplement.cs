/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 14:04:10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FH.ResManagement
{
    internal class ResMgrImplement : IResMgr, IResPool, ICPtr
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        public IResMgr.IExternalLoader _external_loader;

        public IResMgr.Config _conf;
        public ResPool _res_pool;
        public GameObjectInstPool _gobj_pool;
        public ResJobDB _job_db;
        public GameObjectStat _gobj_stat;
        public GameObjectPreInstData _gobj_pre_data;
        public ResInstGc _gc;
        public AtlasLoader _atlas_loader;

        public GameObjectCreatorAsync _worker_go_async;
        public GameObjectCreatorSync _worker_go_sync;
        public ResLoaderAsync _worker_res_async;
        public ResLoaderSync _worker_res_sync;
        public ResInstCallEvent _worker_res_cb;
        public GameObjectPreInst _worker_go_pre_inst;
        public ResMsgQueue _msg_queue;

        static ResMgrImplement()
        {
            MyEqualityComparer.Reg(ResId.EqualityComparer);
            MyEqualityComparer.Reg(ResPath.EqualityComparer);
            MyEqualityComparer.Reg(ResRef.EqualityComparer);
        }

        public ResMgrImplement(IResMgr.IExternalLoader external_loader, IResMgr.Config conf)
        {
            _external_loader = external_loader;
            _conf = conf;

            _gobj_stat = new GameObjectStat();
            _res_pool = new ResPool();
            _gobj_pool = new GameObjectInstPool(_gobj_stat);
            _job_db = new ResJobDB();
            _gobj_pre_data = new GameObjectPreInstData(conf.PreInst);
            _msg_queue = new ResMsgQueue();
            _msg_queue._job_db = _job_db;

            _gc = new ResInstGc(conf.GC);
            _gc._res_pool = _res_pool;
            _gc._gobj_pool = _gobj_pool;
            _gc._gobj_pre_data = _gobj_pre_data;
            _gc._gobj_stat = _gobj_stat;

            _worker_go_async = new GameObjectCreatorAsync(conf.MaxAsyncGameObjectStep);
            _worker_go_async._res_pool = _res_pool;
            _worker_go_async._gobj_pool = _gobj_pool;
            _worker_go_async._job_db = _job_db;
            _worker_go_async._msg_queue = _msg_queue;
            _worker_go_async.Init();

            _worker_go_sync = new GameObjectCreatorSync();
            _worker_go_sync._res_pool = _res_pool;
            _worker_go_sync._gobj_pool = _gobj_pool;
            _worker_go_sync._msg_queue = _msg_queue;
            _worker_go_sync.Init();

            _worker_res_async = new ResLoaderAsync(conf.MaxAsyncResLoaderSlot);
            _worker_res_async._res_pool = _res_pool;
            _worker_res_async._external_loader = external_loader;
            _worker_res_async._job_db = _job_db;
            _worker_res_async._msg_queue = _msg_queue;
            _worker_res_async._res_pool_interface = this;
            _worker_res_async.Init();

            _worker_res_sync = new ResLoaderSync();
            _worker_res_sync._res_pool = _res_pool;
            _worker_res_sync._external_loader = external_loader;
            _worker_res_sync._msg_queue = _msg_queue;
            _worker_res_sync.Init();

            _worker_res_cb = new ResInstCallEvent();
            _worker_res_cb._msg_queue = _msg_queue;
            _worker_res_cb.Init();

            _worker_go_pre_inst = new GameObjectPreInst(conf.PreInst.Priority);
            _worker_go_pre_inst._gobj_stat = _gobj_stat;
            _worker_go_pre_inst._gobj_pre_data = _gobj_pre_data;
            _worker_go_pre_inst._job_db = _job_db;
            _worker_go_pre_inst._msg_queue = _msg_queue;

            _atlas_loader = new AtlasLoader();
            _atlas_loader._external_loader = _external_loader;
            _atlas_loader._job_db = _job_db;
            _atlas_loader._res_pool = _res_pool;
            _atlas_loader._msg_queue = _msg_queue;
            _atlas_loader.Init();
        }

        public void Update()
        {
            _msg_queue.ProcessMsgs(int.MaxValue);
            _worker_res_async.Update();
            _worker_go_async.Update();
            _gc.Update();
            _worker_go_pre_inst.Update();
        }

        public void Destroy()
        {
            ResLog._.D("ResMgrImplement Destroy Begin");
            _atlas_loader.Destroy();
            _worker_go_async.Destroy();
            _worker_go_sync.Destroy();
            _worker_res_async.Destroy();
            _worker_res_sync.Destroy();
            _worker_res_cb.Destroy();
            _job_db.Destroy();

            _gobj_pool.Destroy();
            _res_pool.Destroy();

            _atlas_loader = null;
            _worker_go_async = null;
            _worker_go_sync = null;
            _worker_res_async = null;
            _worker_res_sync = null;
            _worker_res_cb = null;
            _job_db = null;
            _gobj_pool = null;
            _res_pool = null;
            _gc = null;
            _msg_queue.ClearMsgQueue();
            _msg_queue = null;
            ___ptr_ver++;
            ResLog._.D("ResMgrImplement Destroy End");
        }

        public ResRef GetResRef(UnityEngine.Object res)
        {
            if (res == null)
                return default;
            int inst_id = res.GetInstanceID();

            if (inst_id < 0)
            {
                {
                    ResId res_id = new ResId(inst_id, EResType.Inst);
                    if (_gobj_pool.GetInstPath(res_id, out var path) == EResError.OK)
                    {
                        return new ResRef(res_id, path, this);
                    }
                }

                {
                    if (_res_pool.GetPathById(inst_id, out var path) == EResError.OK)
                    {
                        ResId res_id = new ResId(inst_id, EResType.Res);
                        return new ResRef(res_id, path.Path, this);
                    }
                }
            }
            else
            {
                {
                    if (_res_pool.GetPathById(inst_id, out var path) == EResError.OK)
                    {
                        ResId res_id = new ResId(inst_id, EResType.Res);
                        return new ResRef(res_id, path.Path, this);
                    }
                }
                {
                    ResId res_id = new ResId(inst_id, EResType.Inst);
                    if (_gobj_pool.GetInstPath(res_id, out var path) == EResError.OK)
                    {
                        return new ResRef(res_id, path, this);
                    }
                }
            }
            return default;
        }

        public EResError AddUser(ResId res_id, System.Object user)
        {
            //0.check
            if (!res_id.IsValid())
            {
                ResLog._.I("add user，id_ref null");
                return EResError.OK;
            }

            switch (res_id.ResType)
            {
                case EResType.Res:
                    {
                        EResError err2 = _res_pool.AddUser(res_id.Id, user);
                        ResLog._.ErrCode(err2, "add user ，{0} 类型的资源", res_id.ResType);
                        return err2;
                    }

                case EResType.Inst:
                    {
                        EResError err3 = _gobj_pool.AddUsage(res_id, user);
                        ResLog._.ErrCode(err3, "add user ，实例类型的资源");
                        return err3;
                    }
                default:
                    return EResError.Error;
            }
        }

        public EResError RemoveUser(ResId res_id, System.Object user)
        {
            //0.check
            if (!res_id.IsValid())
            {
                ResLog._.Assert(false, "remove user，id_ref null");
                return EResError.OK;
            }

            switch (res_id.ResType)
            {
                case EResType.Res:
                    {
                        EResError err2 = _res_pool.RemoveUser(res_id.Id, user);
                        ResLog._.ErrCode(err2, "remove user ，{0} 类型的资源, {1}", res_id.ResType, res_id.Id);
                        return err2;
                    }

                case EResType.Inst:
                    {
                        EResError err3 = _gobj_pool.RemoveUser(res_id, user);
                        ResLog._.ErrCode(err3, "remove user ，实例类型的资源");
                        return err3;
                    }
                default:
                    return EResError.Error;
            }
        }

        public UnityEngine.Object Get(ResId res_id)
        {
            //0.check
            if (!res_id.IsValid())
            {
                ResLog._.E("Null ResId");
                return null;
            }

            switch (res_id.ResType)
            {
                default:
                    return null;

                case EResType.Res:
                    return _res_pool.GetRes(res_id.Id);

                case EResType.Inst:
                    return _gobj_pool.GetInst(res_id, true);
            }
        }

        public T Get<T>(ResId res_id) where T : UnityEngine.Object
        {
            UnityEngine.Object obj = Get(res_id);
            if (obj == null)
                return null;

            T ret = obj as T;
            ResLog._.Assert(ret != null, "类型不对，当前类型 {0}, 你要的 {1}", obj.GetType(), typeof(T));
            return ret;
        }

        #region Res
        public EResError Load(string path, EResPathType pathType, bool aync_load_enable, out ResRef res_ref)
        {
            //1. check
            if (string.IsNullOrEmpty(path))
            {
                res_ref = default;
                ResLog._.Assert(false, "路径为空 ");
                return (EResError)EResError.ResMgrImplement_path_null_2;
            }

            //2. 先尝试找一下
            ResPath res_path = ResPath.Create(path, pathType);
            EResError err = _res_pool.GetIdByPath(res_path, out var res_id);
            if (err == EResError.OK) //如果找到了，直接返回
            {
                res_ref = new ResRef(res_id, path, this);
                return EResError.OK;
            }
            if (!aync_load_enable)
            {
                res_ref = default;
                return err;
            }

            //3. 同步加载
            ResJob job = _job_db.CreateJob(res_path, 0);
            job.AddWorker(EResWoker.sync_load_res);
            _msg_queue.BeginJob(job, true);

            //4. 再次找到该资源
            err = _res_pool.GetIdByPath(res_path, out res_id);
            //ResLog._.ErrCode(err, "找不到资源,可能不存在 {0}", path);
            if (err == EResError.OK)
                res_ref = new ResRef(res_id, path, this);
            else
                res_ref = default;
            return err;
        }
        public EResError AsyncLoad(string path, EResPathType pathType, int priority, IResDoneCallBack cb, out int job_id)
        {
            return AsyncLoad(path, pathType, priority, ResDoneEvent.Create(cb), out job_id);
        }
        public EResError AsyncLoad(string path, EResPathType pathType, int priority, ResEvent cb, out int job_id)
        {
            return AsyncLoad(path, pathType, priority, ResDoneEvent.Create(cb), out job_id);
        }
        public EResError AsyncLoad(string path, EResPathType pathType, int priority, AwaitableCompletionSource<(EResError error, ResRef res_ref)> source, CancellationToken cancelToken)
        {
            return AsyncLoad(path, pathType, priority, ResDoneEvent.Create(source, cancelToken), out _);
        }
        public EResError AsyncLoad(string path, EResPathType pathType, int priority, ResDoneEvent resEvent, out int job_id)
        {
            //1. check
            if (string.IsNullOrEmpty(path))
            {
                job_id = 0;
                ResLog._.Assert(false, "路径为空");
                return (EResError)EResError.ResMgrImplement_path_null_4;
            }
            if (!resEvent.IsValid)
            {
                job_id = 0;
                ResLog._.Assert(false, "回调为空");
                return (EResError)EResError.ResMgrImplement_call_back_null;
            }

            //2. 异步加载
            ResJob job = _job_db.CreateJob(ResPath.Create(path, pathType), priority);
            job.EventResCallBack = resEvent;
            job.AddWorker(EResWoker.async_load_res);
            job.AddWorker(EResWoker.call_res_event);

            _msg_queue.BeginJob(job, false);
            job_id = job.JobId;

            return EResError.OK;
        }

        public EAssetStatus GetAssetStatus(string res_path)
        {
            return _external_loader.GetAssetStatus(res_path);
        }

        // 资源的部分,key: path
        public void ResSnapshot(ref List<ResSnapShotItem> out_snapshot)
        {
            _res_pool.Snapshot(ref out_snapshot);
        }
        #endregion

        #region Inst
        public EResError GetInstPath(ResId inst_id, out string path)
        {
            return _gobj_pool.GetInstPath(inst_id, out path);
        }

        //释放使用权之后， 当使用者为0时候，立即释放
        public EResError TryDestroyInst(ResId inst_id)
        {
            //1. 获取资源的信息
            bool suc1 = _gobj_pool.GetInstResUser(
                  inst_id
                  , out string path
                  , out UnityEngine.GameObject inst
                  , out int inst_user_count
                  , out System.Object res_user);

            if (!suc1)
                return EResError.OK;
            if (inst_user_count > 0)
                return EResError.OK;


            //2. 移除对资源的依赖
            EResError err = _res_pool.RemoveUser(ResPath.CreateRes(path), res_user, out ResId _);

            //3. 移除
            bool suc2 = _gobj_pool.RemoveInst(inst_id, out string _);
            GoUtil.Destroy(inst);
            return EResError.OK;
        }

        //优先查找是否有空余的
        public EResError Create(string path, System.Object user, bool aync_load_enable, out ResRef res_ref)
        {
            //1. check
            if (string.IsNullOrEmpty(path))
            {
                ResLog._.Assert(false, "路径为空");
                res_ref = default;
                return (EResError)EResError.ResMgrImplement_path_null_5;
            }
            if (user == null)
            {
                ResLog._.Assert(false, "user 为空");
                res_ref = default;
                return (EResError)EResError.ResMgrImplement_user_null_1;
            }

            //2. 判断是否有空余的
            EResError err = _gobj_pool.PopInst(path, user, out var inst_id);
            if (err == EResError.OK)
            {
                res_ref = new ResRef(inst_id, path, this);
                return err;
            }

            //3. 加载
            ResJob job = _job_db.CreateJob(ResPath.CreateRes(path), 0);
            if (!aync_load_enable) //不允许同步加载
            {
                err = _res_pool.GetIdByPath(ResPath.CreateRes(path), out job.ResId);
                if (err != EResError.OK) //资源不存在, 没有加载
                {
                    _msg_queue.SendJobNext(job);
                    res_ref = default;
                    return err;
                }
            }
            else
            {
                job.AddWorker(EResWoker.sync_load_res);
            }
            job.AddWorker(EResWoker.sync_obj_inst);
            _msg_queue.BeginJob(job, true);

            //4. 再次检查
            err = _gobj_pool.PopInst(path, user, out inst_id);
            if (err == EResError.OK)
            {
                res_ref = new ResRef(inst_id, path, this);
                return err;
            }
            else
            {
                res_ref = default;
                return err;
            }
        }

        public EResError TryCreate(string path, System.Object user, out ResRef res_ref)
        {
            //1. check
            if (string.IsNullOrEmpty(path))
            {
                ResLog._.Assert(false, "路径为空");
                res_ref = default;
                return (EResError)EResError.ResMgrImplement_path_null_5;
            }
            if (user == null)
            {
                ResLog._.Assert(false, "user 为空");
                res_ref = default;
                return (EResError)EResError.ResMgrImplement_user_null_1;
            }

            //2. 判断是否有空余的
            EResError err = _gobj_pool.PopInst(path, user, out var inst_id);
            if (err == EResError.OK)
            {
                res_ref = new ResRef(inst_id, path, this);
                return err;
            }
            else
            {
                res_ref = default;
                return err;
            }
        }
        public EResError AsyncCreate(string path, int priority, IInstDoneCallBack cb, out int job_id)
        {
            return AsyncCreate(path, priority, InstDoneEvent.Create(cb), out job_id);
        }

        public EResError AsyncCreate(string path, int priority, InstEvent cb, out int job_id)
        {
            return AsyncCreate(path, priority, InstDoneEvent.Create(cb), out job_id);
        }

        public EResError AsyncCreate(string path, int priority, AwaitableCompletionSource<EResError> source, CancellationToken cancelToken)
        {
            var ret= AsyncCreate(path, priority, InstDoneEvent.Create(source,cancelToken), out var job_id);
            //CancelJob(job_id); 
            return ret;
        }

        public EResError AsyncCreate(string path, int priority, InstDoneEvent instDoneEvent, out int job_id)
        {
            //1. check
            if (string.IsNullOrEmpty(path))
            {
                ResLog._.Assert(false, "路径为空");
                job_id = 0;
                return (EResError)EResError.ResMgrImplement_path_null_7;
            }
            if (!instDoneEvent.IsValid)
            {
                job_id = 0;
                ResLog._.Assert(false, "回调为空");
                return (EResError)EResError.ResMgrImplement_call_back_null2;
            }

            //2. 创建Job
            ResJob job = _job_db.CreateJob(ResPath.CreateRes(path), priority);
            job.EventInstCallBack = instDoneEvent;
            job.AddWorker(EResWoker.async_load_res);
            job.AddWorker(EResWoker.async_obj_inst);
            job.AddWorker(EResWoker.call_inst_event);

            //3. 开始加载
            _msg_queue.BeginJob(job, false);
            job_id = job.JobId;
            return EResError.OK;
        }
        #endregion

        #region 预实例化
        public EResError ReqPreInst(string path, int count, out int req_id)
        {
            //1. 检查参数
            if (string.IsNullOrEmpty(path))
            {
                req_id = 0;
                return (EResError)EResError.ResMgrImplement_path_null_8;
            }

            //2. 检查资源是否存在
            var asset_status = _external_loader.GetAssetStatus(path);
            if (asset_status != EAssetStatus.Exist)
            {
                req_id = 0;
                return (EResError)EResError.ResMgrImplement_res_not_exist;
            }

            //3. 添加
            return _gobj_pre_data.Req(path, count, out req_id);
        }

        public EResError CancelPreInst(int req_id)
        {
            return _gobj_pre_data.Cancel(req_id);
        }
        #endregion

        public void CancelJob(int job_id)
        {
            _job_db.CancelJob(job_id);
        }
    }
}
