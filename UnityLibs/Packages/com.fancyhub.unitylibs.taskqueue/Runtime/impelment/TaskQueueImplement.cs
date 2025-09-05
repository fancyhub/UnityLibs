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
    internal sealed class TaskQueueImple : ITaskQueue
    {
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        private const string C_THREAD_NAME_FORMATER = "TaskMgr_{0}";
        private int _max_thread_count = 0;

        //这里是等待被处理的task，还没有进入线程处理
        private LinkedList<TaskImplement> _pending_queue = new LinkedList<TaskImplement>();

        private List<TaskWorker> _thread_pool;
        private ETaskQueueStatus _status = ETaskQueueStatus.None;
        private ThreadPriority _priority = ThreadPriority.Normal;

        //子线程的数量
        public void Start(int thread_count)
        {
            if (_status != ETaskQueueStatus.None)
                return;

            if (thread_count < 1)
                return;

            _max_thread_count = thread_count;
            _status = ETaskQueueStatus.Running;
            _thread_pool = new List<TaskWorker>(thread_count);
            for (int i = 0; i < thread_count; ++i)
            {
                _thread_pool.Add(new TaskWorker(i));
            }
        }


        public ThreadPriority Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                if (_priority == value)
                    return;
                _priority = value;
                foreach (var t in _thread_pool)
                {
                    t.Priority = _priority;
                }
            }
        }

        public void Stop()
        {
            if (_status == ETaskQueueStatus.None)
                return;
            _status = ETaskQueueStatus.Stoping;
        }

        public ETaskQueueStatus GetStatus()
        {
            return _status;
        }

        public void Destroy()
        {
            ___obj_ver++;
            for (int i = 0; i < _thread_pool.Count; ++i)
            {
                _thread_pool[i].Destroy();
            }
            _thread_pool.Clear();
        }

        public TaskRef AddTask(Action task, Action call_back = null)
        {
            TaskImplement ret = TaskImplement.Create(task, call_back);
            _pending_queue.ExtAddLast(ret);
            ret._status = ETaskStatus.Pending;
            return new TaskRef(ret);
        }

        //清除所有在等待队列里面的任务
        public void ClearPendingTask()
        {
            _pending_queue.Clear();
        }

        //这里处理回调函数,需要在主线程里面调用
        public void Update()
        {
            for (int i = 0; i < _thread_pool.Count; ++i)
            {
                TaskWorker tw = _thread_pool[i];
                if (!tw.IsAlive())
                {
                    _thread_pool[i].Destroy();
                    _thread_pool[i] = new TaskWorker(i);
                }

                _pending_queue.ExtPeekFirst(out TaskImplement task);

                if (_thread_pool[i].ProcessTask(task))
                {
                    _pending_queue.ExtRemoveFirst();
                }
            }
        }
    }
}
