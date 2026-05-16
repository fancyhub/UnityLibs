/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/12
 * Title   :
 * Desc    :
*************************************************************************************/

using System.Threading;

namespace FH
{
    internal sealed class TaskWorker
    {
        private const string CThreadName = "TaskQueueWorker_";

        private enum EWorkerStatus
        {
            None,
            Wait,
            Run,
            Finish,
        }

        private Semaphore _semaphore;
        private Thread _thread;
        private TaskImplement _task;
        private volatile int _status = (int)EWorkerStatus.None;

        public TaskWorker(int index)
        {
            _semaphore = new Semaphore(0, 1);
            _thread = new Thread(WorkerProc);
            _thread.Name = CThreadName + index;
            _thread.IsBackground = true;
            _thread.Start();
        }

        public bool IsIdle => _status == (int)EWorkerStatus.None;

        public ThreadPriority Priority
        {
            set
            {
                var t = _thread;
                if (t == null || !t.IsAlive)
                    return;
                t.Priority = value;
            }
        }

        public bool IsAlive()
        {
            var t = _thread;
            return t != null && t.IsAlive;
        }

        public bool ProcessTask(TaskImplement new_task)
        {
            TaskImplement finish_task = TakeFinishTask();
            FinishTask(finish_task);

            if (new_task == null)
                return false;

            if (_status != (int)EWorkerStatus.None)
                return false;

            _task = new_task;
            _status = (int)EWorkerStatus.Wait;
            _semaphore.Release();

            return true;
        }

        private TaskImplement TakeFinishTask()
        {
            if (_status != (int)EWorkerStatus.Finish)
                return null;

            TaskImplement ret = _task;
            _task = null;
            _status = (int)EWorkerStatus.None;
            return ret;
        }

        //线程运行的
        private void WorkerProc()
        {
            for (; ; )
            {
                _semaphore.WaitOne();

                if (_status != (int)EWorkerStatus.Wait)
                    continue;

                _status = (int)EWorkerStatus.Run;
                _task?.RunInWorkThread();
                _status = (int)EWorkerStatus.Finish;
            }
        }

        private static void FinishTask(TaskImplement task)
        {
            if (task == null)
                return;

            task.CallBack();
            task.Destroy();
        }
    }
}
