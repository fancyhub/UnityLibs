/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;

namespace FH
{
    public enum ETaskQueueStatus
    {
        none,   //没有运行
        running, //正在运行
        stoping, //正在关闭，等待当前的任务做完，如果当前任务是无限循环，理论上是关闭不了
    }


    //状态迁移
    /*
    None -> Pending : 主线程设置，创建完立刻就是pending
    Pending -> Run: 子线程设置
    Run -> Finish: 子线程设置 (会执行任务，然后设置状态)
    Finish -> Dead: 主线程设置 （会先设置，然后调用回调)    
    Dead -> Destroyed: 主线程设置，会回收到池里面
    */
    public enum ETaskStatus
    {
        None,
        Pending,
        Run,
        Finish,
        Dead,
        Destroyed,
    }

    public interface ITask : ICPtr
    {
        public ETaskStatus Status { get; }
        public void Cancel();
    }

    public interface ITaskQueue : ICPtr
    {
        public void Start(int thread_count);
        public void Stop();

        public void ClearPendingTask();
        public ThreadPriority Priority { get; set; }
        public ETaskQueueStatus GetStatus();

        public ITask AddTask(Action task, Action main_thread_call_back = null);
        public void Update();
    }


    public static class TaskQueue
    {
        private static CPtr<ITaskQueue> _;
        private static bool _ManualUpdate;
        public static void Init(int thread_count, bool manual_update = false)
        {
            if (!_.Null)
            {
                Log.E("TaskQueue Can't init twice");
                return;
            }

            TaskQueueImple task_queue = new TaskQueueImple();
            task_queue.Start(thread_count);
            _ = new CPtr<ITaskQueue>(task_queue);
            _ManualUpdate = manual_update;
            TaskQueueUpdater.CreateInst(task_queue, manual_update);
        }

        /// <summary>
        /// 只能在主线程调用
        /// </summary>
        public static UnityEngine.Coroutine StartCoroutine(System.Collections.IEnumerator routine)
        {
            return TaskQueueUpdater.CreateInst().StartCoroutine(routine);
        }

        /// <summary>
        /// 只能在主线程调用
        /// </summary>
        public static void StopCoroutine(UnityEngine.Coroutine routine)
        {
            TaskQueueUpdater.CreateInst().StopCoroutine(routine);
        }

        public static ETaskQueueStatus Status
        {
            get
            {
                if (_.Null)
                {
                    Log.E("TaskQueue Is Null");
                    return ETaskQueueStatus.none;
                }
                return _.Val.GetStatus();
            }
        }

        public static ITask AddTask(Action task, Action main_thread_call_back = null)
        {
            if (_.Null)
            {
                Log.E("TaskQueue Is Null");
                return null;
            }
            return _.Val.AddTask(task, main_thread_call_back);
        }


        public static ThreadPriority Priority
        {
            get
            {
                if (_.Null)
                {
                    Log.E("TaskQueue Is Null");
                    return default;
                }
                return _.Val.Priority;
            }
            set
            {
                if (_.Null)
                {
                    Log.E("TaskQueue Is Null");
                    return;
                }
                _.Val.Priority = value;
            }
        }

        public static void Stop()
        {
            _.Val?.Stop();
        }

        public static void Update()
        {
            if (_.Val == null)
            {
                Log.E("TaskQueue Is Null");
                return;
            }

            if (!_ManualUpdate)
                return;
            _.Val.Update();
        }
    }
}
