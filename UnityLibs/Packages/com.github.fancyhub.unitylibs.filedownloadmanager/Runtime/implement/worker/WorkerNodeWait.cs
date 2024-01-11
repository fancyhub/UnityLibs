using System;
using System.Collections.Generic;
using System.IO;

namespace FH.FileDownload
{

    internal sealed class WorkerNodeWait : IFsmStateNode<WorkerContext, EWorkerMsg, int>
    {
        public const int ResultNext = 1;

        public void Destroy()
        {
        }

        public void OnEnter(WorkerContext context)
        {
        }

        public void OnExit(WorkerContext context)
        {
        }

        public EFsmProcResult OnMsg(WorkerContext context, EWorkerMsg msg, out int result)
        {
            result = default;
            if (msg != EWorkerMsg.Update)
                return EFsmProcResult.None;

            if(!context.RequestJob())            
                return EFsmProcResult.None;

            result = ResultNext;
            return EFsmProcResult.Channged;
        }
    }     
}
