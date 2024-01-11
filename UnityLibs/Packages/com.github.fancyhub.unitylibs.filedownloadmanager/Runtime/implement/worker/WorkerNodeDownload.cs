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
    internal sealed class WorkerNodeDownload : IFsmStateNode<WorkerContext, EWorkerMsg, int>
    {
        public const int ResultSucc = 1;
        public const int ResultFail = 2;
        public const int ResultUserCancel = 3;

        public void Destroy()
        {
        }

        public void OnEnter(WorkerContext context)
        {
            if (context.StartDownload())
                return;
            context._Fsm.SendMsg(EWorkerMsg.Fail);
        }

        public void OnExit(WorkerContext context)
        {
        }

        public EFsmProcResult OnMsg(WorkerContext context, EWorkerMsg msg, out int result)
        {
            switch (msg)
            {
                case EWorkerMsg.Cancel:
                    FileDownloadLog.D("File 取消, {0}", context._RemoteFileUri);
                    context.RequestCancel();
                    result = default;
                    return EFsmProcResult.None;

                case EWorkerMsg.Fail:
                    FileDownloadLog.E("File 下载失败 {0}/{1},{2}", context._Config.RetryCount - context._RetryCount, context._Config.RetryCount, context._RemoteFileUri);

                    if (context._HttpDownloaderError.Error == EHttpDownloaderError.UserCancer)
                    {
                        context.FinishJob(EFileDownloadStatus.Pause);

                        result = ResultUserCancel;
                        return EFsmProcResult.Channged;
                    }
                    else
                    {
                        if (!context.StartDownload())
                        {
                            result = ResultFail;
                            context.FinishJob(EFileDownloadStatus.Failed);
                            return EFsmProcResult.Channged;
                        }
                        else
                        {
                            result = default;
                            return EFsmProcResult.None;
                        }
                    }

                case EWorkerMsg.Succ:
                    FileDownloadLog.D("File 下载成功, {0}", context._RemoteFileUri);
                    context.FinishJob(EFileDownloadStatus.Succ);
                    result = ResultSucc;
                    return EFsmProcResult.Channged;

                default:
                    result = default;
                    return EFsmProcResult.None;
            }
        }
    }
}
