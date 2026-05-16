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
        private int _max_thread_count = 0;
        private int _worker_index = 0;

        private readonly LinkedList<TaskImplement> _pending_queue = new LinkedList<TaskImplement>();
        private readonly List<TaskWorker> _thread_pool = new List<TaskWorker>();
        private ETaskQueueStatus _status = ETaskQueueStatus.None;
        private ThreadPriority _priority = ThreadPriority.Normal;

        public void Start(int max_thread_count)
        {
            if (_status != ETaskQueueStatus.None)
                return;

            if (max_thread_count < 1)
                return;

            _max_thread_count = max_thread_count;
            _worker_index = 0;
            _status = ETaskQueueStatus.Running;
            if (_thread_pool.Capacity < max_thread_count)
                _thread_pool.Capacity = max_thread_count;
        }

        public ThreadPriority Priority
        {
            get { return _priority; }
            set
            {
                if (_priority == value)
                    return;

                _priority = value;
                foreach (TaskWorker worker in _thread_pool)
                {
                    worker.Priority = _priority;
                }
            }
        }

        public ETaskQueueStatus GetStatus()
        {
            return _status;
        }

        public TaskRef AddTask(Action task, Action call_back = null)
        {
            if (_status != ETaskQueueStatus.Running)
                return default;

            TaskImplement ret = TaskImplement.Create(task, call_back);
            ret.SetPending();
            _pending_queue.ExtAddLast(ret);
            return new TaskRef(ret);
        }

        public void ClearPendingTask()
        {
            foreach (TaskImplement task in _pending_queue)
            {
                task.CancelAndDestroy();
            }
            _pending_queue.Clear();
        }

        public void Update()
        {
            if (_status == ETaskQueueStatus.None)
                return;

            for (int i = 0; i < _thread_pool.Count; ++i)
            {
                TaskWorker worker = _thread_pool[i];
                worker.ProcessTask(null);

                if (!worker.IsAlive())
                {
                    _thread_pool.RemoveAt(i);
                    --i;
                    continue;
                }

                DispatchOne(worker);
            }

            while (_pending_queue.Count > 0 && _thread_pool.Count < _max_thread_count)
            {
                TaskWorker worker = new TaskWorker(_worker_index++);
                worker.Priority = _priority;
                _thread_pool.Add(worker);

                DispatchOne(worker);

                if (worker.IsIdle)
                    break;
            }
        }

        private void DispatchOne(TaskWorker worker)
        {
            if (!worker.IsIdle)
                return;

            if (!_pending_queue.ExtPeekFirst(out TaskImplement task))
                return;

            if (worker.ProcessTask(task))
                _pending_queue.ExtRemoveFirst();
        }
    }
}
