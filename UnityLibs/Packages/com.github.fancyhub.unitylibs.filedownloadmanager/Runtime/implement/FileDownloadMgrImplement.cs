/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;

namespace FH.FileDownload
{
    internal sealed class FileDownloadMgrImplement : IFileDownloadMgr
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        public JobDB _JobDB;
        public WorkerFsm[] _Workers;
        public WorkerConfig _WorkerConfig;

        public FileDownloadMgrImplement(IFileDownloadMgr.Config config)
        {
            _JobDB = new JobDB();
            _Workers = new WorkerFsm[config.WorkerCount];
            _WorkerConfig = new WorkerConfig()
            {
                RetryCount = config.MaxRetryCount,
                MaxWorkerCount = config.WorkerCount,
                DownloadTempDir = FileSetting.DownloadDir,
            };

            for (int i = 0; i < config.WorkerCount; i++)
            {
                _Workers[i] = new WorkerFsm(_WorkerConfig, _JobDB, i);
            }
        }

        public void Update()
        {
            foreach (var p in _Workers)
            {
                p.Update();
            }
        }

        public void SetCallBack(FileDownloadCallBack callback)
        {
            _WorkerConfig.CallBack = callback;            
        }

        public void ClearAll()
        {
            _JobDB.ClearAll();
            foreach (var p in _Workers)
            {
                p.Cancel();
            }
        }

        public void GetAllJobs(List<FileDownloadJobInfo> out_list)
        {
            if (out_list == null)
            {
                FileDownloadLog.Assert(false, "param out_list is null");
                return;
            }
            out_list.Clear();
            foreach (var p in _JobDB._Dict)
            {
                out_list.Add(p.Value.Value._JobInfo);
            }
        }

        public FileDownloadJobInfo FindInfo(string job_key_name)
        {
            var job = _JobDB.FindJob(job_key_name);
            if (job == null)
                return null;
            return job._JobInfo;
        }

        public FileDownloadJobInfo AddJob(FileDownloadJobDesc job_desc)
        {
            if (!job_desc.IsValid())
            {
                FileDownloadLog.D("param task is not valid");
                return null;
            }

            var job = _JobDB.AddJob(job_desc);
            if (job == null)
                return null;
            var ret = job._JobInfo;
            if (ret != null)
            {
                _WorkerConfig.CallBack?.Invoke(ret);
            }
            return ret;
        }

        public void Pause(string job_key_name)
        {
            var job = _JobDB.FindJob(job_key_name);
            if (job == null)
                return;

            if (job._WorkerIndex < 0)
            {
                _JobDB.Change(job, EFileDownloadStatus.Pause);
            }
            else
            {
                _Workers[job._WorkerIndex].Cancel();
            }
        }

        /// <summary>
        /// Fail -> pending, Pause -> pending
        /// </summary>
        public void Start(string file_full_name)
        {
            var job = _JobDB.FindJob(file_full_name);
            if (job == null)
                return;

            _JobDB.Change(job, EFileDownloadStatus.Pending);
        }

        public void Destroy()
        {
            ___ptr_ver++;
        }
    }
}
