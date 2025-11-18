/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.ResManagement
{
    //异步加载的task
    internal class GoAsyncCreateTask
    {
        public enum EStatus
        {
            Idle,
            InProcess,
            Done,
        }

        public GameObject prefab;
        public ResJob job;

        private AsyncInstantiateOperation<GameObject> _operation;
        private AssetPool _asset_pool;
        private GameObjectInstPool _gobj_pool;
        private ResMsgQueue _msg_queue;

        public GoAsyncCreateTask(AssetPool asset_pool, GameObjectInstPool gobj_pool, ResMsgQueue msg_queue)
        {
            _asset_pool = asset_pool;
            _gobj_pool = gobj_pool;
            _msg_queue = msg_queue;
        }

        public EStatus GetStatus()
        {
            if (_operation == null)
                return EStatus.Idle;

            if (_operation.isDone)
                return EStatus.Done;

            return EStatus.InProcess;
        }

        public void Start()
        {
            //1. 如果不是preinst的job, 需要判断 当前pool是否有空余的
            if (!job.PreInstJob)
            {
                var err = _gobj_pool.PopInst(job.Path.Path, out job.InstId);
                if (err == EResError.OK)
                {
                    _asset_pool.RemoveUser(job.AssetId, job);
                    _msg_queue.SendJobNext(job);
                    return;
                }
            }

            //2. 真正的实例化第一步
            _operation = GameObjectPoolUtil.InstNewAsync(job.Path.Path, prefab);
            if (null == _operation)
            {
                job.ErrorCode = (EResError)EResError.GameObjectCreatorAsync_inst_error_unkown;
                ResLog._.Assert(false, "实例化的时候， 未知错误 {0}", job.Path.Path);

                _asset_pool.RemoveUser(job.AssetId, job);
                _msg_queue.SendJobNext(job);
                return;
            }
        }

        public void Update()
        {
            //1. 检查operation
            if (_operation == null || !_operation.isDone)
                return;


            //2. 获取实例
            GameObject inst = _GetResult();

            //3. 激活实例
            GameObjectPoolUtil.InstRename(inst, prefab);

            //4. 添加到pool
            bool succ = _gobj_pool.AddInst(new ResRef(job.AssetId, job.Path.Path, _asset_pool), inst, out job.InstId);
            ResLog._.Assert(succ, "添加go inst 到 pool 失败 {0}", job.Path.Path);

            //5. 把当前任务发送给下一个
            _asset_pool.RemoveUser(job.AssetId, job);
            _msg_queue.SendJobNext(job);

            //6. 清除
            prefab = null;
            _operation = null;
            job = null;
        }

        public void Destroy()
        {
            if (_operation == null)
                return;

            if (_operation.isDone)
            {
                foreach (var p in _operation.Result)
                {
                    if (p != null)
                        GameObject.Destroy(p);
                }
                return;
            }
            else
            {
                _operation.completed += (op) =>
                {
                    foreach (var p in _operation.Result)
                    {
                        if (p != null)
                            GameObject.Destroy(p);
                    }
                };
            }
        }

        private GameObject _GetResult()
        {
            if (_operation == null)
                return null;
            if (!_operation.isDone)
                return null;
            var objs = _operation.Result;
            if (objs == null || objs.Length == 0)
                return null;
            return objs[0];
        }
    }


    //异步的创建过程
    internal class GameObjectCreatorAsync : IMsgProc<ResJob>
    {
        #region  传入的    
        public AssetPool _asset_pool;
        public GameObjectInstPool _gobj_pool;
        public ResMsgQueue _msg_queue;
        public ResJobDB _job_db;
        #endregion



        //优先级的job queue
        public ResJobQueuePriority _job_queue;

        public List<ResJob> _temp_list = new List<ResJob>();
        private GoAsyncCreateTask[] _go_tasks;

        public GameObjectCreatorAsync(int worker_count)
        {
            worker_count = Math.Max(worker_count, 1);
            _go_tasks = new GoAsyncCreateTask[worker_count];

        }

        public void Init()
        {
            _job_queue = new ResJobQueuePriority();
            _msg_queue.Reg((int)EResWoker.async_obj_inst, this);

            for (int i = 0; i < _go_tasks.Length; i++)
            {
                _go_tasks[i] = new GoAsyncCreateTask(_asset_pool, _gobj_pool, _msg_queue);
            }
        }

        public void Destroy()
        {
            _msg_queue.UnReg((int)EResWoker.async_obj_inst);
            _job_queue.Destroy();

            foreach (var p in _go_tasks)
                p?.Destroy();
        }

        public void OnMsgProc(ref ResJob job)
        {
            //1. 如果任务取消了，就从db里面移除            
            if (job.IsCancelled)
            {
                _msg_queue.SendJobNext(job);
                return;
            }

            //2.判断一定是自己
            EResWoker job_type = job.GetCurrentWorker();
            if (job_type != EResWoker.async_obj_inst)
            {
                ResLog._.Assert(false, "job 不是 类型 {0}!={1}, {2}", job_type, EResWoker.async_obj_inst, job.Path.Path);
                _msg_queue.SendJobNext(job);
                return;
            }

            //3. 判断是否有错误
            if (job.ErrorCode != EResError.OK)
            {
                _msg_queue.SendJobNext(job);
                return;
            }

            //4.要特殊处理，先添加引用
            EResError err = _asset_pool.AddUser(job.AssetId, job);
            if (err != EResError.OK)
            {
                //直接发到下一个
                job.ErrorCode = (EResError)EResError.GameObjectCreatorAsync_inst_error_asset_null;
                _msg_queue.SendJobNext(job);
                return;
            }

            //5.把任务 压到队列里面
            _job_queue.Add(job.JobId, job.Priority);
        }

        public void Update()
        {
            foreach (var p in _go_tasks)
            {
                //1. 更新
                p.Update();

                //2. 获取状态
                var status = p.GetStatus();
                if (status != GoAsyncCreateTask.EStatus.Idle)
                    continue;

                //3. 找到任务                    
                bool succ = _PopNextJob(
                    _job_queue
                    , _job_db
                    , _asset_pool
                    , out p.job
                    , out p.prefab
                    , ref _temp_list);

                //4. 清除过期的job
                for (int j = 0; j < _temp_list.Count; ++j)
                {
                    ResJob job = _temp_list[j];

                    _asset_pool.RemoveUser(job.AssetId, job);
                    _msg_queue.SendJobNext(job);
                }
                _temp_list.Clear();

                //5. 开始异步实例化
                if (succ)
                    p.Start();
            }
        }

        public static bool _PopNextJob(
            ResJobQueuePriority job_queue
            , ResJobDB job_db
            , AssetPool asset_pool
            , out ResJob out_job
            , out GameObject out_prefab
            , ref List<ResJob> job_list_expire)
        {
            out_job = null;
            out_prefab = null;
            job_list_expire.Clear();

            for (; ; )
            {
                //1. 从队列获取下一个
                int job_id;
                if (!job_queue.Peek(out job_id)) //队列里面没有任务，直接返回
                    return false;
                job_queue.Pop();

                //2. 找到任务 的实例
                bool suc = job_db.Find(job_id, out ResJob job);
                if (!suc)
                {
                    ResLog._.Assert(false, "严重错误,找不到 job {0}", job_id);
                    continue;
                }

                //3. 任务 已经取消了
                if (job.IsCancelled)
                {
                    job_list_expire.Add(job);
                    continue;
                }

                //4. 找到资源，并检查资源                
                UnityEngine.Object res = asset_pool.Get(job.AssetId);
                if (res == null)
                {
                    ResLog._.Assert(false, "严重错误， 资源没有，不能实例化 {0}", job.Path.Path);
                    job.ErrorCode = (EResError)EResError.GameObjectCreatorAsync_inst_error_asset_null2;
                    job_list_expire.Add(job);
                    continue;
                }

                GameObject prefab = res as GameObject;
                if (null == prefab)
                {
                    ResLog._.Assert(false, "实例化的时候， 资源不是 GameObject {0}", job.Path.Path);
                    job.ErrorCode = (EResError)EResError.GameObjectCreatorAsync_inst_error_not_gameobj;
                    job_list_expire.Add(job);
                    continue;
                }

                out_job = job;
                out_prefab = prefab;
                return true;
            }
        }
    }
}
