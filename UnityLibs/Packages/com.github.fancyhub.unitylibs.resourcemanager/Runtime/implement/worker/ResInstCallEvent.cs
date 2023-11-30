/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/12 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;

namespace FH.Res
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
            if (job.IsCanceled || job.EventCallBack == null)
            {
                _msg_queue.SendJobNext(job);
                return;
            }

            EResWoker job_type = job.GetCurrentWorker();
            if (job_type == EResWoker.call_inst_event)
            {
                job.EventCallBack(job.ErrorCode, job.Path, EResType.Inst, job.JobId);
            }
            else if (job_type == EResWoker.call_res_event)
            {
                if (job.Path.Sprite)
                    job.EventCallBack(job.ErrorCode, job.Path, EResType.Sprite, job.JobId);
                else
                    job.EventCallBack(job.ErrorCode, job.Path, EResType.Res, job.JobId);
            }
            else
            {
                ResLog._.E("");
            }

            _msg_queue.SendJobNext(job);
        }
    }
}
