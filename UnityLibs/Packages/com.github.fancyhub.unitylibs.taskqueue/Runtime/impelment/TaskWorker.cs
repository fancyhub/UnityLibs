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
    internal class TaskWorker
    {
        public const string C_THREAD_NAME_FORMATER = "TaskWroker_{0}";

        /*
        正常的流程： 初始化none
        none -> wait 只能在主线程里面修改，同时会设置任务
        wait -> run: 只能在工作线程里面修改
        run -> fin: 只能在工作线程里面修改
        fin -> none: 只能在主线程里面修改,同时把 _task =null        
        */
        public enum E_WORKER_STATUS
        {
            none,   //没有任务
            wait, //已经设置了任务，但是线程还没有开始
            run, //正在执行任务
            fin, // 任务执行完毕，等待结束后的回调
        }

        public E_WORKER_STATUS _status;
        public TaskImplement _task;
        public Thread _thread;
        public Semaphore _semaphore;

        public TaskWorker(int index)
        {
            _task = null;
            _status = E_WORKER_STATUS.none;
            _semaphore = new Semaphore(0, 1);
            _thread = new Thread(_worker);
            _thread.Name = string.Format(C_THREAD_NAME_FORMATER, index);
            _thread.Start();
        }

        public void Destroy()
        {
            _thread?.Abort();
            _thread = null;
            _semaphore?.Dispose();
            _semaphore = null;
            _task = null;
            _status = E_WORKER_STATUS.none;
        }

        public ThreadPriority Priority
        {
            set
            {
                if (!_thread.IsAlive)
                    return;
                _thread.Priority = value;
            }
        }

        public bool IsAlive()
        {
            return _thread.IsAlive;
        }

        //返回的结果    
        public bool ProcessTask(TaskImplement new_task)
        {
            //1. 先处理旧的回调        
            if (_status == E_WORKER_STATUS.fin)
            {
                //先获取旧的，并把状态清除
                TaskImplement old_task = _task;
                _task = null;
                _status = E_WORKER_STATUS.none;

                old_task?.CallBack();
                old_task.Destroy();
            }

            //2. 设置新的
            if (_status == E_WORKER_STATUS.none && new_task != null)
            {
                _task = new_task;
                //设置了任务，修改状态
                _status = E_WORKER_STATUS.wait;
                _semaphore.Release();
                return true;
            }
            return false;
        }

        public void _worker()
        {
            for (; ; )
            {
                //1. 等待信号量
                _semaphore.WaitOne();
                //2. 检查状态， 下面的两种情况都有问题
                if (_status != E_WORKER_STATUS.wait)
                    continue;
                if (_task == null)
                {
                    _status = E_WORKER_STATUS.fin;
                    continue;
                }

                //3. 设置变量
                _status = E_WORKER_STATUS.run;

                _task.Start();
                _status = E_WORKER_STATUS.fin;

            }
        }
    }
}
