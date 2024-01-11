/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

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
                ServerUrl = config.ServerUrl,
                RetryCount = config.MaxRetryCount,
                MaxWorkerCount = config.WorkerCount,
            };

            if (_WorkerConfig.ServerUrl != null)
            {
                if (_WorkerConfig.ServerUrl.EndsWith('/') || _WorkerConfig.ServerUrl.EndsWith('\\'))
                {
                    _WorkerConfig.ServerUrl += FileSetting.Platform.ToString() + "/";
                }
                else
                {
                    _WorkerConfig.ServerUrl += "/" + FileSetting.Platform.ToString() + "/";
                }
            }


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

        public void ClearAll()
        {
            _JobDB.ClearAll();
            foreach (var p in _Workers)
            {
                p.Cancel();
            }
        }

        public void Download(FileManifest.FileItem file)
        {
            if (file == null)
            {
                FileDownloadLog.E("param file is null");
                return;
            }
            if (string.IsNullOrEmpty(file.FullName))
            {
                FileDownloadLog.E("param file.FullName is null");
                return;
            }

            string local_path = System.IO.Path.Combine(FileSetting.LocalDir, file.FullName);
            if (System.IO.File.Exists(local_path))
            {
                FileDownloadLog.D("File {0} has Downloaded", file.FullName);
                return;
            }
            _JobDB.AddJob(file);
        }

        public void Pause(FileManifest.FileItem file)
        {
            var job = _JobDB.FindJob(file);
            if (job == null)
                return;

            if (job.WorkerIndex < 0)
            {
                _JobDB.Change(job, EFileDownloadStatus.Pause);
                return;
            }
            _Workers[job.WorkerIndex].Cancel();
        }

        public void Destroy()
        {
            ___ptr_ver++;
        }
    }
}
