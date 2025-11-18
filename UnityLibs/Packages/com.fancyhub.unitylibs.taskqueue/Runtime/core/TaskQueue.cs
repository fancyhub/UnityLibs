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
        None,   //没有运行
        Running, //正在运行
        Stoping, //正在关闭，等待当前的任务做完，如果当前任务是无限循环，理论上是关闭不了
    }


    //状态迁移
    /*
    None -> Pending : 主线程设置，创建完立刻就是pending
    Pending -> Running: 子线程设置
    Running -> WaitCallBack: 子线程设置 (会执行任务，然后设置状态)
    WaitCallBack -> Dead: 主线程设置 （会先设置，然后调用回调)        
    */
    public enum ETaskStatus
    {
        None,
        Pending,
        Running,
        WaitCallBack,
        Finish,
    }

    public interface ITask : ICPtr
    {
        public ETaskStatus Status { get; }
        public void Cancel();
        public bool IsDestroyed();
    }

    public struct TaskRef
    {
        private ITask _Task;
        private readonly int _Id;
        internal TaskRef(ITask task)
        {
            _Task = task;
            _Id = _Task == null ? 0 : _Task.ObjVersion;
        }

        public bool IsValid() { return Task != null; }

        public ITask Task
        {
            get
            {
                if (_Task == null)
                    return null;
                if (_Task.ObjVersion != _Id)
                {
                    _Task = null;
                    return null;
                }
                return _Task;
            }
        }
        public bool IsDone()
        {
            var t = Task;
            if (t == null)
                return true;
            if (t.IsDestroyed() || t.Status == ETaskStatus.Finish)
                return true;
            return false;
        }

        public void Cancel()
        {
            Task?.Cancel();
        }
    }

    public interface ITaskQueue : ICPtr
    {
        public void Start(int thread_count);
        public void Stop();

        public void ClearPendingTask();
        public ThreadPriority Priority { get; set; }
        public ETaskQueueStatus GetStatus();

        public TaskRef AddTask(Action task, Action main_thread_call_back = null);
        public void Update();
    }


    public static class TaskQueue
    {
        private static CPtr<ITaskQueue> _;
        private static bool _ManualUpdate;
        public static void Init(int max_thread_count = 0, bool manual_update = false)
        {
            if (!_.Null)
            {
                Log.E("TaskQueue Can't init twice");
                return;
            }

            TaskQueueImple task_queue = new TaskQueueImple();
            if (max_thread_count <= 0)
            {
                max_thread_count = System.Environment.ProcessorCount * 2;
            }
            task_queue.Start(max_thread_count);
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
                    return ETaskQueueStatus.None;
                }
                return _.Val.GetStatus();
            }
        }

        public static TaskRef AddTask(Action task, Action main_thread_call_back = null)
        {
            if (_.Null)
            {
                Log.E("TaskQueue Is Null");
                return default;
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
