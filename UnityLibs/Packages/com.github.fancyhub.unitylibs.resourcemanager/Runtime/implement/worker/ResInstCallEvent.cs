/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;

namespace FH.ResManagement
{
    //发出event
    internal class ResInstCallEvent : IMsgProc<ResJob>
    {
        #region  传入的    
        public ResMsgQueue _msg_queue;
        #endregion

        public void Init()
        {
            //注册两个
            _msg_queue.Reg(EResWoker.call_inst_event, this);
            _msg_queue.Reg(EResWoker.call_res_event, this);
        }

        public void Destroy()
        {
            _msg_queue.UnReg(EResWoker.call_inst_event);
            _msg_queue.UnReg(EResWoker.call_res_event);
        }

        public void OnMsgProc(ref ResJob job)
        {
            if (!job.EventResCallBack.IsValid && !job.EventInstCallBack.IsValid)
            {
                _msg_queue.SendJobNext(job);
                return;
            }

            if (job.ErrorCode == EResError.OK && job.IsCancelled)
                job.ErrorCode = EResError.UserCancelled;

            ResLog._.ErrCode(job.ErrorCode,job.Path.Path);
            EResWoker job_type = job.GetCurrentWorker();
            if (job_type == EResWoker.call_inst_event)
            {
                job.EventInstCallBack.Call(job.JobId, job.ErrorCode, job.Path.Path);
            }
            else if (job_type == EResWoker.call_res_event)
            {
                job.EventResCallBack.Call(job.JobId, job.ErrorCode, job.ResRef);
            }
            else
            {
                ResLog._.E("");
            }
            _msg_queue.SendJobNext(job);
        }
    }
}
