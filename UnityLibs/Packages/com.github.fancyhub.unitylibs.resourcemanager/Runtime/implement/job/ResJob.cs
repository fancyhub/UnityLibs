/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FH.ResManagement
{
    internal enum EResWoker
    {
        none,
        sync_load_res,  //同步加载
        sync_obj_inst,  //同步实例化

        async_load_res, //异步加载
        async_obj_inst, //异步实例化        

        call_res_event,
        call_inst_event,

        call_set_atlas,
        max,
    }

    internal struct ResDoneEvent
    {
        public int Type;
        public ResEvent Action;
        public CPtr<IResDoneCallBack> Target;
        public AwaitableCompletionSource<(EResError error, ResRef res_ref)> AwaitSource;
        public CancellationToken CancelToken;

        public bool IsValid => Type != 0;


        public static ResDoneEvent Create(ResEvent action)
        {
            if (action == null)
                return default;
            ResDoneEvent ret = new ResDoneEvent()
            {
                Type = 1,
                Action = action,
                Target = null,
            };
            return ret;
        }

        public static ResDoneEvent Create(IResDoneCallBack target)
        {
            if (target == null)
                return default;

            ResDoneEvent ret = new ResDoneEvent()
            {
                Type = 2,
                Action = null,
                Target = new CPtr<IResDoneCallBack>(target),
            };
            return ret;
        }

        public static ResDoneEvent Create(AwaitableCompletionSource<(EResError error, ResRef res_ref)> source, CancellationToken token)
        {
            if (source == null)
                return default;
            ResDoneEvent ret = new ResDoneEvent()
            {
                Type = 3,
                Action = null,
                Target = null,
                AwaitSource = source,
                CancelToken = token,
            };
            return ret;
        }

        public void Call(int job_id, EResError error, ResRef res_ref)
        {
            try
            {
                switch (Type)
                {
                    case 1:
                        Action(job_id, error, res_ref);
                        break;
                    case 2:
                        Target.Val?.OnResDoneCallback(job_id, error, res_ref);
                        break;
                    case 3:
                        this.AwaitSource.TrySetResult((error, res_ref));
                        break;
                }
            }
            catch (Exception e)
            {
                ResLog._.E(e);
            }
        }
    }

    internal struct InstDoneEvent
    {
        public int Type;
        public InstEvent Action;
        public CPtr<IInstDoneCallBack> Target;
        public AwaitableCompletionSource<(EResError error, ResRef res_ref)> AwaitSource;
        public CancellationToken CancelToken;

        public bool IsValid => Type != 0;

        public static InstDoneEvent Create(InstEvent action)
        {
            if (action == null)
                return default;

            InstDoneEvent ret = new InstDoneEvent()
            {
                Type = 1,
                Action = action,
                Target = null,
                AwaitSource = null,
                CancelToken = default,
            };
            return ret;
        }

        public static InstDoneEvent Create(IInstDoneCallBack target)
        {
            if (target == null)
                return default;
            InstDoneEvent ret = new InstDoneEvent()
            {
                Type = 2,
                Action = null,
                Target = new CPtr<IInstDoneCallBack>(target),
                AwaitSource = null,
                CancelToken = default,
            };
            return ret;
        }

        public static InstDoneEvent Create(AwaitableCompletionSource<(EResError error, ResRef res_ref)> source, CancellationToken token)
        {
            if (source == null)
                return default;
            InstDoneEvent ret = new InstDoneEvent()
            {
                Type = 3,
                Action = null,
                Target = null,
                AwaitSource = source,
                CancelToken = token,
            };
            return ret;
        }

        public void Call(int job_id, EResError error, ResRef res_ref)
        {
            try
            {
                switch (Type)
                {
                    case 1:
                        Action(job_id, error, res_ref);
                        break;
                    case 2:
                        Target.Val?.OnResDoneCallback(job_id, error, res_ref);
                        break;
                    case 3:
                        this.AwaitSource.TrySetResult((error,res_ref));
                        break;
                }
            }
            catch (Exception e)
            {
                ResLog._.E(e);
            }
        }
    }

    //事务
    internal sealed class ResJob : CPoolItemBase
    {
        public int JobId;
        public ResPath Path;
        public int Priority;
        public EResError ErrorCode;
        public bool Immediately = false;
        public bool PreInstJob = false;

        public ResId ResId;
        public ResId InstId;



        public ResDoneEvent EventResCallBack;
        public InstDoneEvent EventInstCallBack;

        private bool _IsCancelled = false;

        private LinkedList<int> _workers = new LinkedList<int>();

        //调试用的
        private LinkedList<int> _done_workers = new LinkedList<int>();

        //取消了事务
        public void Cancel()
        {
            //已经取消了
            if (_IsCancelled)
                return;

            ResLog._.D("res job 被取消了 {0},{1}", JobId, Path.Path);
            _IsCancelled = true;
        }

        public bool IsCancelled
        {
            get
            {
                if (_IsCancelled)
                    return true;
                if (EventResCallBack.IsValid && EventResCallBack.CancelToken.IsCancellationRequested)
                    return true;
                if (EventInstCallBack.IsValid && EventInstCallBack.CancelToken.IsCancellationRequested)
                    return true;
                return false;
            }
        }

        public bool AddWorker(EResWoker job)
        {
            if (_IsCancelled)
            {
                ResLog._.Assert(false, "已经被取消了，不能添加了");
                return false;
            }
            _workers.ExtAddLast((int)job);
            return true;
        }

        public EResWoker GetCurrentWorker()
        {
            if (_workers.Count == 0)
                return EResWoker.none;
            var node = _workers.First;
            return (EResWoker)node.Value;
        }

        public bool MoveWorkerNext()
        {
            if (_workers.Count == 0)
                return false;

            _workers.ExtPopFirst(out int v);
            _done_workers.ExtAddLast(v);

            return _workers.Count > 0;
        }

        protected override void OnPoolRelease()
        {
            _workers.ExtClear();
            _done_workers.ExtClear();

            ResId = ResId.Null;
            InstId = ResId.Null;

            EventResCallBack = default;
            EventInstCallBack = default;

            PreInstJob = false;
            JobId = 0;
            Priority = 0;
            _IsCancelled = false;
            ErrorCode = EResError.OK;            
        }
    }
}
