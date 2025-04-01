/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


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

        public ETaskStatus _status = ETaskStatus.None;
        public bool _cancel = false;
        public Action _task;
        public Action _call_back;

        public static TaskImplement Create(Action task, Action call_back = null)
        {
            TaskImplement ret = GPool.New<TaskImplement>();
            ret._task = task;
            ret._call_back = call_back;
            ret._status = ETaskStatus.None;
            ret._cancel = false;
            return ret;
        }

        public ETaskStatus Status { get { return _status; } }

        public void Destroy()
        {
            //1. 检查状态
            if (_status != ETaskStatus.Dead)
            {
                Log.Assert(false, "状态不合法 destroy {0}", _status);
                return;
            }

            //2.  修改成员变量
            _status = ETaskStatus.Destroyed;
            _task = null;
            _call_back = null;


            if (___pool == null || ___pool.Del(this))
            {
                ___obj_ver++;
                return;
            }
        }

        public void Cancel()
        {
            //1. 这两个状态修改 标记位不合法
            if (_status == ETaskStatus.None
                || _status == ETaskStatus.Destroyed)
            {
                Log.E("Invaldate status to cancel {0}", _status);
                return;
            }

            //2. 不是立即取消的，要等待所有的流程走完
            _cancel = true;
        }

        public void Start()
        {
            //1. 
            if (_status != ETaskStatus.Pending)
            {
                Log.E("Invaldate status to Start {0}", _status);
                return;
            }
            _status = ETaskStatus.Run;

            //2. 检查是否已经取消了
            if (_cancel)
            {
                _status = ETaskStatus.Finish;
                return;
            }

            //3. 真正的执行
            try
            {
                _task?.Invoke();
            }
            catch (System.Threading.ThreadAbortException)
            {
                //这个是推出的时候会触发的错误,先不打印错误了
            }
            catch (Exception e)
            {
                Log.E(e);
            }
            finally
            {
                _status = ETaskStatus.Finish;
            }
        }

        public void CallBack()
        {
            //1. 检查状态
            if (_status != ETaskStatus.Finish)
            {
                Log.E("Invaldate status to CallBack {0}", _status);
                return;
            }
            _status = ETaskStatus.Dead;

            //2. 判断是否已经取消了
            if (_cancel)
                return;

            //3. 调用回调
            _call_back?.Invoke();
        }
    }
}
