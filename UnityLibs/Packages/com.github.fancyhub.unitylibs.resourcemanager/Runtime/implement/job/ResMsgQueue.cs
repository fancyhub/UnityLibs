/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/30
 * Title   : 
 * Desc    : 
*************************************************************************************/


namespace FH.Res
{
    internal class ResMsgQueue : MsgQueue<ResJob>
    {
        public ResJobDB _job_db;

        public bool Reg(EResWoker handle, IMsgProc<ResJob> target)
        {
            return this.Reg((int)handle, target);
        }

        public void UnReg(EResWoker handle)
        {
            this.UnReg((int)handle);
        }

        public void BeginJob(ResJob job, bool immediately)
        {
            job.Immediately = immediately;
            _SendJob(job, false);
        }

        //把job 发给下一个worker
        public void SendJobNext(ResJob job)
        {
            _SendJob(job, true);
        }

        //把job 发给下一个worker
        private void _SendJob(ResJob job, bool move_next)
        {
            //1. 检查
            if (job == null)
                return;
            if (null == _job_db)
                return;

            //2. 判断是否已经取消了            
            int job_id = job.JobId;
            if (job.IsCanceled)
            {
                _job_db.Remove(job_id);
                return;
            }

            //3. 移动job到下一步
            if (move_next && !job.MoveWorkerNext())
            {
                //没有后续步骤了
                _job_db.Remove(job_id);
                return;
            }

            //4. 根据任务类型，找到下一个worker
            EResWoker worker = job.GetCurrentWorker();
            ResLog._.Assert(worker != EResWoker.none);
            bool succ = this.Find((int)worker) != null;
            if (!succ)
            {
                ResLog._.Assert(false, "找不到 Worker 类型： {0}", worker);
                _job_db.Remove(job_id);
                return;
            }

            //5. 发消息
            this.SendTo((int)worker, ref job, job.Immediately);
        }
    }
}
