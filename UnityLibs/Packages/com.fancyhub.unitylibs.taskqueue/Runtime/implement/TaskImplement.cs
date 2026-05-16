/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/12
 * Title   :
 * Desc    :
*************************************************************************************/

using System;

namespace FH
{
    internal sealed class TaskImplement : ITask, IPoolItem, ICPtr
    {
        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }

        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        private volatile int _status = (int)ETaskStatus.None;
        private volatile int _cancel = 0;
        private volatile int _destroyed = 0;

        private Action _task;
        private Action _call_back;

        public static TaskImplement Create(Action task, Action call_back = null)
        {
            TaskImplement ret = GPool.New<TaskImplement>();
            ret._task = task;
            ret._call_back = call_back;
            ret.SetStatus(ETaskStatus.None);
            ret._cancel = 0;
            ret._destroyed = 0;
            return ret;
        }

        public ETaskStatus Status => GetStatus();
        public bool IsDestroyed() { return _destroyed != 0; }

        public void Cancel()
        {
            ETaskStatus status = GetStatus();
            if (status == ETaskStatus.None || status == ETaskStatus.Finish || IsDestroyed())
            {
                Log.E("Invaldate status to cancel {0}", status);
                return;
            }

            _cancel = 1;
        }

        public void Destroy()
        {
            ETaskStatus status = GetStatus();
            if (status != ETaskStatus.Finish || IsDestroyed())
            {
                Log.Assert(false, "状态不合法 destroy {0}", status);
                return;
            }

            _destroyed = 1;
            _task = null;
            _call_back = null;
            ___obj_ver++;

            if (___pool != null)
                ___pool.Del(this);
        }

        internal void SetPending()
        {
            SetStatus(ETaskStatus.Pending);
        }

        internal void CancelAndDestroy()
        {
            if (IsDestroyed())
                return;

            _cancel = 1;

            ETaskStatus status = GetStatus();
            if (status != ETaskStatus.Finish)
                SetStatus(ETaskStatus.WaitCallBack);

            CallBack();
            Destroy();
        }

        internal void RunInWorkThread()
        {
            ETaskStatus status = GetStatus();
            if (status != ETaskStatus.Pending && status != ETaskStatus.Running)
            {
                Log.E("Invaldate status to Start {0}", status);
                SetStatus(ETaskStatus.WaitCallBack);
                return;
            }

            SetStatus(ETaskStatus.Running);

            try
            {
                if (!IsCanceled())
                    _task?.Invoke();
            }
            catch (Exception e)
            {
                Log.E(e);
            }
            finally
            {
                SetStatus(ETaskStatus.WaitCallBack);
            }
        }

        public void CallBack()
        {
            ETaskStatus status = GetStatus();
            if (status == ETaskStatus.Finish)
                return;

            if (status != ETaskStatus.WaitCallBack)
            {
                Log.E("Invaldate status to CallBack {0}", status);
                return;
            }

            SetStatus(ETaskStatus.Finish);

            if (IsCanceled())
                return;

            try
            {
                _call_back?.Invoke();
            }
            catch (Exception e)
            {
                Log.E(e);
            }
        }

        private ETaskStatus GetStatus()
        {
            return (ETaskStatus)_status;
        }

        private void SetStatus(ETaskStatus status)
        {
            _status = (int)status;
        }

        private bool IsCanceled()
        {
            return _cancel != 0;
        }
    }
}
