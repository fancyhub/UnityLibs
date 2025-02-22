/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

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

    //事务
    internal sealed class ResJob : CPoolItemBase
    {
        public int JobId;
        public ResPath Path;
        public int Priority;
        public EResError ErrorCode;
        public bool IsCanceled = false;
        public bool Immediately = false;

        public ResId ResId;
        public ResRef ResRef;

        public ResEvent EventResCallBack;
        public InstEvent EventInstCallBack;

        private LinkedList<int> _workers = new LinkedList<int>();

        //调试用的
        private LinkedList<int> _done_workers = new LinkedList<int>();

        //取消了事务
        public void Cancel()
        {
            //已经取消了
            if (IsCanceled)
                return;

            ResLog._.D("res job 被取消了 {0},{1}", JobId, Path.Path);
            IsCanceled = true;
        }

        public bool AddWorker(EResWoker job)
        {
            if (IsCanceled)
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
            EventResCallBack = null;
            EventInstCallBack = null;

            JobId = 0;
            Priority = 0;
            IsCanceled = false;
            ErrorCode = EResError.OK;
            ResRef = default;
        }
    }
}
