/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using FH.Res;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.Res
{
    //异步加载的task
    internal class ResAsyncTask
    {
        public ResPath _path;
        public IAssetRef _asset_request;
        public List<int> _job_ids;

        public ResAsyncTask()
        {
            _job_ids = new List<int>();
        }

        public void Clear()
        {
            if (_asset_request != null)
            {
                _asset_request.Destroy();
                _asset_request = null;
            }
            _path = default;
            _job_ids.Clear();
        }
    }

    //资源异步加载
    internal class ResLoaderAsync : IMsgProc<ResJob>
    {
        #region  传入的
        public ResPool _res_pool;
        public IAssetLoader _asset_loader;
        public ResJobDB _job_db;
        public ResMsgQueue _msg_queue;
        #endregion



        //最大的数量
        public const int C_MAX_COUNT = 10;
        public int _count = 5;
        public ResAsyncTask[] _task_slots = new ResAsyncTask[C_MAX_COUNT];

        //优先级的job queue
        public ResJobQueuePriority _job_queue;

        public ResLoaderAsync()
        {
            for (int i = 0; i < _task_slots.Length; ++i)
            {
                _task_slots[i] = new ResAsyncTask();
            }
            _job_queue = new ResJobQueuePriority();
        }

        public void Init()
        {
            //1. 先把自己注册到 message queue 上
            _msg_queue.Reg(EResWoker.async_load_res, this);
        }

        public void Destroy()
        {
            _msg_queue.UnReg(EResWoker.async_load_res);
            _job_queue.Destroy();
            foreach (ResAsyncTask task in _task_slots)
            {
                task.Clear();
            }
        }

        public void SetWorkerCount(int count)
        {
            _count = System.Math.Max(count, C_MAX_COUNT);
        }

        public void OnMsgProc(ref ResJob job)
        {
            //1. 如果任务取消了，就从db里面移除
            if (job.IsCanceled)
            {
                _msg_queue.SendJobNext(job);
                return;
            }

            //3.判断一定是自己
            EResWoker job_type = job.GetCurrentWorker();
            if (job_type != EResWoker.async_load_res)
            {
                ResLog._.Assert(false, "job 不是 类型 {0}!={1}, {2}", job_type, EResWoker.async_load_res, job.Path.Path);
                _msg_queue.SendJobNext(job);
                return;
            }

            //4.判断是否已经添加了
            EResError err = _res_pool.GetIdByPath(job.Path, out job.ResId);
            if (err == EResError.OK)
            {
                //刷新一下, 不要被回收了
                _res_pool.AddUser(job.ResId.Id, job);
                _res_pool.RemoveUser(job.ResId.Id, job);

                //直接发到下一个
                _msg_queue.SendJobNext(job);
                return;
            }

            //5. 判断资源是否存在
            var asset_status = _asset_loader.GetAssetStatus(job.Path.Path);
            if (asset_status != EAssetStatus.Exist)
            {
                job.ErrorCode = EResError.ResLoaderAsync_res_not_exist;
                ResLog.ErrCode(job.ErrorCode, $"资源不存在 {job.Path.Path} {asset_status}");

                //直接发到下一个
                _msg_queue.SendJobNext(job);
                return;
            }

            //6.把任务 压到队列里面
            _job_queue.Add(job.JobId, job.Priority);
        }

        public void Update()
        {
            //1. 先把当前正在运行的任务 弄完
            for (int i = 0; i < _task_slots.Length; ++i)
            {
                ResAsyncTask task = _task_slots[i];

                //1.1 检查是否任务完成了
                if (!task._path.IsValid())
                    continue;
                ResLog._.Assert(task._asset_request != null, "任务 为空 {0}", task._path);
                if (!task._asset_request.IsDone)
                    continue;

                //1.2 添加到pool里面
                ResLog._.D("load res async {0}", task._path);
                EResError error_code = _res_pool.AddRes(task._path, task._asset_request, out ResId _);
                ResLog.ErrCode(error_code, $"添加资源失败 {task._path}");
                if (error_code != EResError.OK)
                {
                    task._asset_request.Destroy();
                    task._asset_request = null;
                }

                //1.3 把下面的所有job 发到下一个worker里面去
                foreach (int job_id in task._job_ids)
                {
                    _job_db.Find(job_id, out ResJob job);
                    job.ErrorCode = error_code;
                    _msg_queue.SendJobNext(job);
                }

                //1.5 清除任务
                task.Clear();
            }

            //2. 再从队列里面获取下一个任务
            for (; ; )
            {
                //2.1 先获取任务id，不要取出
                int job_id;
                if (!_job_queue.Peek(out job_id))
                    break;

                //2.2 找到任务的内容
                bool succ = _job_db.Find(job_id, out ResJob job);
                if (!succ)
                {
                    ResLog._.Assert(false, "找不到job {0}", job_id);
                    _job_queue.Pop();
                    continue;
                }

                //2.3 判断job 是否已经被取消了                
                if (job.IsCanceled)
                {
                    _job_queue.Pop();
                    _msg_queue.SendJobNext(job);
                    continue;
                }

                //2.4 判断资源是否已经加载好了
                EResError err = _res_pool.GetIdByPath(job.Path, out job.ResId);
                if (err == EResError.OK)
                {
                    //刷新一下
                    _res_pool.AddUser(job.ResId.Id, job);
                    _res_pool.RemoveUser(job.ResId.Id, job);

                    _job_queue.Pop();
                    _msg_queue.SendJobNext(job);
                    continue;
                }

                //2.5 找到执行的task_slot
                ResAsyncTask task_slot = _GetTaskSlot(_task_slots, _count, job.Path);
                if (null == task_slot)
                {
                    //没有空余的 task slot，直接break
                    break;
                }

                //2.6  把自己的job id 挂到 task slot下面
                _job_queue.Pop();
                task_slot._job_ids.Add(job_id);

                //2.7 判断task 有没有开始加载 ，因为有可能多个任务加载同一个资源，会需要合并
                if (task_slot._path.IsValid())
                    continue;

                task_slot._path = job.Path;
                task_slot._asset_request = _asset_loader.LoadAsync(job.Path.Path, job.Path.Sprite);


                if (task_slot._asset_request != null)
                    continue;

                //2.8 加载失败
                ResLog._.Assert(false, "加载资源的asset request为空 {0}", job.Path.Path);
                task_slot.Clear();
                job.ErrorCode = EResError.ResLoaderAsync_load_res_failed2;
                _msg_queue.SendJobNext(job);
            }
        }

        //找到能加载 该资源的 task slot
        public static ResAsyncTask _GetTaskSlot(ResAsyncTask[] task_slot, int count, ResPath path)
        {
            //1. 先找相同的
            for (int i = 0; i < task_slot.Length; ++i)
            {
                if (task_slot[i]._path == path)
                    return task_slot[i];
            }

            //2. 找空的
            for (int i = 0; i < count; ++i)
            {
                if (task_slot[i]._path == null)
                    return task_slot[i];
            }
            return null;
        }
    }
}