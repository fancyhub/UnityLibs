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
    //异步的创建过程
    internal class GameObjectCreatorAsync : IMsgProc<ResJob>
    {
        #region  传入的    
        public ResPool _res_pool;
        public GameObjectInstPool _gobj_pool;
        public ResJobDB _job_db;
        public ResMsgQueue _msg_queue;
        #endregion



        //优先级的job queue
        public ResJobQueuePriority _job_queue;

        //一帧执行多少步
        public int _step_count_per_frame = 1;

        public ResJob _current_job;
        private GameObject _obj_inst;
        public List<ResJob> _temp_list = new List<ResJob>();

        public GameObjectCreatorAsync(int step_count_per_frame)
        {
            _step_count_per_frame = Math.Max(1, step_count_per_frame);
        }

        public void Init()
        {
            _job_queue = new ResJobQueuePriority();
            _msg_queue.Reg((int)EResWoker.async_obj_inst, this);
        }

        public void Destroy()
        {
            _msg_queue.UnReg((int)EResWoker.async_obj_inst);

            if (null != _obj_inst)
            {
                GoUtil.Destroy(_obj_inst);
                _obj_inst = null;
            }
            _job_queue.Destroy();
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
            EResError err = _res_pool.AddUser(job.ResId, job);
            if (err != EResError.OK)
            {
                //直接发到下一个
                job.ErrorCode = (EResError)EResError.GameObjectCreatorAsync_inst_error_res_null;
                _msg_queue.SendJobNext(job);
                return;
            }

            //5.把任务 压到队列里面
            _job_queue.Add(job.JobId, job.Priority);
        }

        public void Update()
        {
            //一次只能执行几个步骤
            for (int i = 0; i < _step_count_per_frame; ++i)
            {
                //实例化第一步
                if (null == _current_job)
                {
                    //1. 先找到任务
                    GameObject prefab;
                    bool succ = _PopNextJob(
                        _job_queue
                        , _job_db
                        , _res_pool
                        , out _current_job
                        , out prefab
                        , ref _temp_list);

                    //2. 清除过期的job
                    for (int j = 0; j < _temp_list.Count; ++j)
                    {
                        ResJob job = _temp_list[j];

                        _res_pool.RemoveUser(job.ResId, job);
                        _msg_queue.SendJobNext(job);
                    }
                    _temp_list.Clear();

                    //3. 判断是否有新的任务
                    if (!succ)
                        return;

                    //4. 如果不是preinst的job, 需要判断 当前pool是否有空余的
                    if (!_current_job.PreInstJob)
                    {
                        var err = _gobj_pool.PopInst(_current_job.Path.Path, out _current_job.InstId);
                        if (err == EResError.OK)
                        {
                            _res_pool.RemoveUser(_current_job.ResId, _current_job);
                            _msg_queue.SendJobNext(_current_job);
                            _current_job = null;
                            continue;
                        }
                    }


                    //5. 真正的实例化第一步
                    _obj_inst = GameObjectPoolUtil.InstNew(_current_job.Path.Path, prefab);
                    if (null == _obj_inst)
                    {
                        _current_job.ErrorCode = (EResError)EResError.GameObjectCreatorAsync_inst_error_unkown;
                        ResLog._.Assert(false, "实例化的时候， 未知错误 {0}", _current_job.Path.Path);

                        _res_pool.RemoveUser(_current_job.ResId, _current_job);
                        _msg_queue.SendJobNext(_current_job);
                        _current_job = null;
                    }
                    continue;
                }

                //实例化第2步
                {
                    //1 获取一些参数
                    GameObject inst = _obj_inst;
                    ResJob job = _current_job;
                    _current_job = null;
                    _obj_inst = null;

                    //2. 激活实例
                    GameObjectPoolUtil.InstActive(inst);

                    //3. 添加到pool
                    bool succ = _gobj_pool.AddInst(new ResRef(job.ResId, job.Path.Path, _res_pool), inst, out job.InstId);
                    ResLog._.Assert(succ, "添加go inst 到 pool 失败 {0}", job.Path.Path);
                    //ResLog._.assert(ResConst.GetIdType(inst_id) == E_RES_TYPE.inst);


                    //4. 把当前任务发送给下一个
                    _res_pool.RemoveUser(job.ResId, job);
                    _msg_queue.SendJobNext(job);
                }
            }
        }

        public static bool _PopNextJob(
            ResJobQueuePriority job_queue
            , ResJobDB job_db
            , ResPool res_pool
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
                UnityEngine.Object res = res_pool.Get(job.ResId);
                if (res == null)
                {
                    ResLog._.Assert(false, "严重错误， 资源没有，不能实例化 {0}", job.Path.Path);
                    job.ErrorCode = (EResError)EResError.GameObjectCreatorAsync_inst_error_res_null2;
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
