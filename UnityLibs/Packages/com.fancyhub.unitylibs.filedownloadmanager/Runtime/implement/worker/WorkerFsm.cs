/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace FH.FileDownload
{
    internal enum EWorkerState
    {
        Wait,
        Download,
    }

    internal enum EWorkerMsg
    {
        Update,
        Cancel,
        Succ,
        Fail,
    }

    internal sealed class WorkerFsm
    {
        internal WorkerContext _WorkerContext;
        public IFsm<EWorkerMsg> _Fsm;

        public WorkerFsm(WorkerConfig config, JobDB job_db, int index)
        {
            _WorkerContext = new WorkerContext(config, job_db, index);
            var fsm = FsmCreator.CreateFsmWithContext(EFsmMode.Async, _WorkerContext, CreateStateVT(), CreateStateTranMap());

            _WorkerContext.SetFsm(fsm);
            fsm.Start();
            _Fsm = fsm;
        }

        public void Update()
        {
            _Fsm.SendMsg(EWorkerMsg.Update);
            _Fsm.ProcAllMsgs();
        }

        public void Cancel()
        {
            _Fsm.SendMsg(EWorkerMsg.Cancel);
        }

        private static FsmStateTranMap<EWorkerState, int> _StateTranMap;
        public static FsmStateTranMap<EWorkerState, int> CreateStateTranMap()
        {
            if (_StateTranMap != null)
                return _StateTranMap;

            _StateTranMap = new FsmStateTranMap<EWorkerState, int>(EWorkerState.Wait);
            _StateTranMap
                .From(EWorkerState.Wait)
                    .To(WorkerNodeWait.ResultNext, EWorkerState.Download)
                .From(EWorkerState.Download)
                    .To(WorkerNodeDownload.ResultSucc, EWorkerState.Wait)
                    .To(WorkerNodeDownload.ResultFail, EWorkerState.Wait)
                    .To(WorkerNodeDownload.ResultUserCancel, EWorkerState.Wait);

            return _StateTranMap;
        }

        private static FsmStateVTWithContext<WorkerContext, EWorkerState, EWorkerMsg, int> _StateVT;
        public static FsmStateVTWithContext<WorkerContext, EWorkerState, EWorkerMsg, int> CreateStateVT()
        {
            if (_StateVT != null)
                return _StateVT;
            _StateVT = new FsmStateVTWithContext<WorkerContext, EWorkerState, EWorkerMsg, int>(4);
            _StateVT[EWorkerState.Wait] = new WorkerNodeWait();
            _StateVT[EWorkerState.Download] = new WorkerNodeDownload();

            return _StateVT;
        }
    }
}
