/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;

namespace FH.FileDownload
{
    internal class WorkerConfig
    {
        public int RetryCount = 3;
        public int MaxWorkerCount = 0;
        public string DownloadTempDir;

        public FileDownloadCallBack CallBack;
    }

    internal sealed class WorkerContext
    {
        //初始化传入
        public WorkerConfig _Config;
        public IFsm<EWorkerState, EWorkerMsg> _Fsm;
        public JobDB _JobDB;
        public int _WorkerIndex;

        public HttpDownloaderError _HttpDownloaderError;
        public CPtr<Job> _CurrentJob;
        public FileDownloadJobInfo _CurrentJobInfo;
        public FileDownloadJobDesc _CurrentJobDesc;

        public CPtr<ITask> _Task;


        public string _RemoteFileUri; // http://xxxx/Android/file_full_name 
        public string _LocalFilePath; // Application.persistentDataPath/some_folder/file_full_name        
        public string _DownloadFilePath; //Application.persistentDataPath/Download/file_full_name 

        public int _RetryCount;
        public System.Threading.CancellationTokenSource _CancellationTokenSource;
        public System.Threading.CancellationToken _CancellationToken;


        public WorkerContext(WorkerConfig config, JobDB job_db, int worker_index)
        {
            _JobDB = job_db;
            _Config = config;
            _WorkerIndex = worker_index;
        }

        public void SetFsm(IFsm<EWorkerState, EWorkerMsg> fsm)
        {
            _Fsm = fsm;
        }

        public bool RequestJob()
        {
            var job = _JobDB.PopPending();
            if (job == null)
                return false;
            _CurrentJob = job;
            _CurrentJobInfo = job._JobInfo;
            _CurrentJobDesc = _CurrentJobInfo.JobDesc;

            _RemoteFileUri = _CurrentJobDesc.RemoteUrl;
            _LocalFilePath = _CurrentJobDesc.DestFilePath;
            _DownloadFilePath = _Config.DownloadTempDir + _CurrentJobDesc.KeyName;
            _RetryCount = _Config.RetryCount + 1;
            job._WorkerIndex = _WorkerIndex;
            return true;
        }

        public void RequestCancel()
        {
            _CancellationTokenSource?.Cancel();
        }

        public void FinishJob(EFileDownloadStatus status)
        {
            Job job = _CurrentJob;
            if (job == null)
                return;

            _JobDB.Change(job, status);
            job._WorkerIndex = -1;


            _CurrentJob = null;
            _Config.CallBack?.Invoke(job._JobInfo);
        }

        public bool StartDownload()
        {
            //1.  检查当前Job
            Job job = _CurrentJob;
            if (job == null)
            {
                FileDownloadLog.E("Job Is Null");
                return false;
            }

            //2. 创建Cancel Source
            if (_CancellationTokenSource == null || _CancellationTokenSource.IsCancellationRequested)
            {
                _CancellationTokenSource = new System.Threading.CancellationTokenSource();
                _CancellationToken = _CancellationTokenSource.Token;
            }

            //3. 减少重试次数
            _RetryCount--;
            if (_RetryCount < 0)
            {
                FileDownloadLog.D("重试次数到了, 下载失败 {0}", _RemoteFileUri);
                return false;
            }

            //4. 
            ITask task = null;
            FileDownloadLog.D("添加Task {0} -> {1}", _RemoteFileUri, _LocalFilePath);

            if (_RemoteFileUri.StartsWith("http://") || _RemoteFileUri.StartsWith("https://"))
            {
                task = TaskQueue.AddTask(_TaskHttpDownload, _OnDownloaded);
            }
            else
            {
                task = TaskQueue.AddTask(_TaskFileCopy, _OnDownloaded);
            }
            _Task = new CPtr<ITask>();
            return task != null;
        }

        private void _OnDownloaded()
        {
            if (_HttpDownloaderError.Error == EHttpDownloaderError.OK)
            {
                _Fsm.SendMsg(EWorkerMsg.Succ);
            }
            else
            {
                _Fsm.SendMsg(EWorkerMsg.Fail);
            }
        }

        private void _TaskFileCopy()
        {
            FileDownloadLog.D("Start Task {0}", _RemoteFileUri);
            _HttpDownloaderError.Reset();

            try
            {
                FileUtil.CreateFileDir(_LocalFilePath);
                System.IO.File.Copy(_RemoteFileUri, _LocalFilePath);
            }
            catch (IOException e)
            {
                _HttpDownloaderError.SetIOException(e);
            }
        }

        private void _OnHttpDownloadFileSizeCB(long download_size, long total_size)
        {
            if (_CurrentJobInfo == null)
            {
                FileDownloadLog.D("Job Is Null,{0}", _RemoteFileUri);
                return;
            }
            _CurrentJobInfo._DownloadSize = download_size;
        }

        private void _TaskHttpDownload()
        {
            FileDownloadLog.D("Start Task {0}", _RemoteFileUri);

            //1. 重置错误码
            _HttpDownloaderError.Reset();

            //3. 检查是否被取消了
            if (_CancellationToken.IsCancellationRequested)
            {
                FileDownloadLog.D("下载任务被取消了, {0}", _RemoteFileUri);
                _HttpDownloaderError.SetCanceled();
                return;
            }

            //4. 根据GZ 调整路径
            string temp_remote_file_url = _RemoteFileUri;
            string temp_download_file_path = _DownloadFilePath;
            if (_CurrentJobDesc.UseGz)
            {
                temp_remote_file_url += ".gz";
                temp_download_file_path += ".gz";
            }

            //5. 开始下载
            FileDownloadLog.D("开始下载 {0} -> {1}", temp_remote_file_url, temp_download_file_path);
            _HttpDownloaderError = HttpDownloader.Download(
               temp_remote_file_url,
               temp_download_file_path,
               _OnHttpDownloadFileSizeCB,
               _CurrentJobDesc.Crc32,
               _CancellationToken);

            //6. 检查错误
            if (_HttpDownloaderError.Error != EHttpDownloaderError.OK)
            {
                FileDownloadLog.E("下载错误 {0},{1}", _HttpDownloaderError.Error, _RemoteFileUri);
                return;
            }

            //下面的不可取消
            //7. 解压缩
            if (_CurrentJobDesc.UseGz)
            {
                try
                {
                    if (File.Exists(_DownloadFilePath))
                        File.Delete(_DownloadFilePath);

                    {
                        using FileStream fs_in = File.OpenRead(temp_download_file_path);
                        using FileStream fs_out = new FileStream(_DownloadFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                        fs_in.CopyTo(fs_out);
                    }

                    File.Delete(temp_download_file_path);
                }
                catch (IOException e)
                {
                    //可能空间不够
                    _HttpDownloaderError.SetIOException(e);
                    _HttpDownloaderError.PrintLog();
                    return;
                }
                catch (Exception e)
                {
                    _HttpDownloaderError.SetUnkownException(e);
                    _HttpDownloaderError.PrintLog();
                    return;
                }
            }

            //8.移动
            FileUtil.CreateFileDir(_LocalFilePath);
            System.IO.File.Move(_DownloadFilePath, _LocalFilePath);
        }
    }
}
