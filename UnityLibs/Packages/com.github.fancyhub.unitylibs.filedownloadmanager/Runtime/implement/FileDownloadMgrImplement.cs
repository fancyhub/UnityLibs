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

        public void GetAllInfo(List<FileDownloadInfo> out_list)
        {
            if (out_list == null)
            {
                FileDownloadLog.Assert(false, "param out_list is null");
                return;
            }
            out_list.Clear();
            foreach (var p in _JobDB._Dict)
            {
                out_list.Add(p.Value.Value._DwonloadInfo);
            }
        }

        public FileDownloadInfo FindInfo(string file_full_name)
        {
            var job = _JobDB.FindJob(file_full_name);
            if (job == null)
                return null;
            return job._DwonloadInfo;
        }

        public FileDownloadInfo AddTask(FileManifest.FileItem file)
        {
            if (file == null)
            {
                FileDownloadLog.Assert(false, "param file is null");
                return null;
            }
            if (string.IsNullOrEmpty(file.FullName))
            {
                FileDownloadLog.Assert(false, "param file.FullName is null");
                return null;
            }

            string local_path = System.IO.Path.Combine(FileSetting.LocalDir, file.FullName);
            if (System.IO.File.Exists(local_path))
            {
                FileDownloadLog.D("File {0} has Downloaded", file.FullName);
                return new FileDownloadInfo(file.Name, file.FullName, file.Size, EFileDownloadStatus.Succ);

            }
            var job = _JobDB.AddJob(file);
            if (job == null)
                return null;
            return job._DwonloadInfo;
        }

        public void Pause(string file_full_name)
        {
            var job = _JobDB.FindJob(file_full_name);
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
