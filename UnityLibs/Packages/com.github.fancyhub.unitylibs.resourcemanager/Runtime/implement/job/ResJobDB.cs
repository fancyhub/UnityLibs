/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;

namespace FH.ResManagement
{
    internal class ResJobDB
    {
        private static int _job_id_gen = 0;

        public Dictionary<int, ResJob> _dict_job;
        public ResJobDB()
        {
            _dict_job = new Dictionary<int, ResJob>();
        }

        public int GetCount()
        {
            return _dict_job.Count;
        }

        public static int NextJobId
        {
            get
            {
                _job_id_gen++;
                return _job_id_gen;
            }
        }

        public ResJob CreateJob(ResPath path, int priority)
        {
            ResJob ret = GPool.New<ResJob>();
            ret.JobId = NextJobId;
            ret.Path = path;
            ret.Priority = priority;
            ret.ResRef = new ResRef(ResId.Null, path.Path, null);

            _dict_job.Add(ret.JobId, ret);
            return ret;
        }         


        public bool CancelJob(int job_id)
        {
            bool suc = _dict_job.TryGetValue(job_id, out ResJob job);

            if (!suc)
            {
                ResLog._.Assert(false, "找不到job {0}", job_id);
                return false;
            }

            job.Cancel();
            return true;
        }

        public bool Find(int job_id, out ResJob out_mid)
        {
            return _dict_job.TryGetValue(job_id, out out_mid);
        }

        public bool Remove(int job_id)
        {
            ResJob job;
            _dict_job.TryGetValue(job_id, out job);
            bool ret = _dict_job.Remove(job_id);
            if (ret)
                job.Destroy();
            return ret;
        }

        public void Destroy()
        {
            foreach (var p in _dict_job)
            {
                p.Value.Destroy();
            }
            _dict_job.Clear();
        }
    }
}
