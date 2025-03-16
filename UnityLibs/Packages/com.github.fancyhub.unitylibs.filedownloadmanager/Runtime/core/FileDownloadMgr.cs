/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using FH.FileDownload;

namespace FH
{
    public enum EFileDownloadStatus
    {
        Pending, //等待,排队中
        Downloading, //正在下载
        Pause,
        Failed, //下载失败
        Succ, //下载成功
    }

    public struct FileDownloadJobDesc
    {
        public  string KeyName;
        public  string RemoteUrl;   //全路径
        public  string DestFilePath; //全路径
        public  long TotalSize;
        public  uint Crc32;
        public  bool UseGz;
        public System.Object UserData;

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(KeyName))
                return false;
            if (string.IsNullOrEmpty(RemoteUrl))
                return false;
            if (string.IsNullOrEmpty(DestFilePath))
                return false;
            return true;
        }
    }

    public sealed class FileDownloadJobInfo
    {
        public readonly FileDownloadJobDesc JobDesc;

        internal long _DownloadSize;
        internal EFileDownloadStatus _Status;

        internal FileDownloadJobInfo(FileDownloadJobDesc job_desc)
        {
            this.JobDesc = job_desc;            
        }

        public EFileDownloadStatus Status => _Status;
        public long DownloadSize => _DownloadSize;
        public long TotalSize => JobDesc.TotalSize;

        public System.Object UserData => JobDesc.UserData;

        public T GetUserData<T>()
        {
            if (JobDesc.UserData == null)
                return default;

            if (JobDesc.UserData is T ret)
                return ret;
            return default;
        }

        public bool IsValid()
        {
            return JobDesc.IsValid();
        }

        public float Progress
        {
            get
            {
                if (_Status == EFileDownloadStatus.Succ)
                    return 1.0f;
                if (JobDesc.TotalSize <= 0)
                    return 0.99f;
                float p = (float)((double)_DownloadSize / (double)JobDesc.TotalSize);

                return System.Math.Clamp(p, 0, 0.99f);
            }
        }
    }

    public partial interface IFileDownloadMgr : ICPtr
    {
        public void Update();

        public FileDownloadJobInfo AddJob(FileDownloadJobDesc job);

        /// <summary>
        /// Pending -> pause, Downloading -> pause
        /// </summary>
        public void Pause(string job_key_name);

        /// <summary>
        /// Fail -> pending, Pause -> pending <br/>
        /// 有可能失效, 比如当前正在 Downloading, 先调用Pause, 还没有完全暂停, 再次调用 Restart        
        /// </summary>
        public void Start(string job_key_name);

        public FileDownloadJobInfo FindInfo(string job_key_name);

        public void SetCallBack(FileDownloadCallBack callback);

        public void GetAllJobs(List<FileDownloadJobInfo> out_job_list);

        public void ClearAll();
    }

    public static class FileDownloadMgr
    {
        private static CPtr<IFileDownloadMgr> _Inst;

        private static string DefaultServerUrl;
        private static string DefaultSaveDir;
        public static void Init(IFileDownloadMgr.Config config)
        {
            if (config == null)
            {
                FileDownloadLog.E("Config is Null");
                return;
            }
            FileDownloadLog.SetMasks(config.LogLvl);
            if (!_Inst.Null)
            {
                FileDownloadLog.E("FileDownloadMgr can't create twice");
                return;
            }

            if (string.IsNullOrEmpty(config.ServerUrl))
            {
                FileDownloadLog.E("ServerUrl is Null");
                return;
            }

            var file_download_mgr = new FileDownloadMgrImplement(config);
            _Inst = new CPtr<IFileDownloadMgr>(file_download_mgr);
            DefaultServerUrl = config.ServerUrl;

            if (DefaultServerUrl != null)
            {
                if (DefaultServerUrl.EndsWith('/') || DefaultServerUrl.EndsWith('\\'))
                {
                    DefaultServerUrl += FileSetting.Platform.ToString() + "/";
                }
                else
                {
                    DefaultServerUrl += "/" + FileSetting.Platform.ToString() + "/";
                }
            }
            DefaultSaveDir = FileSetting.LocalDir;
        }

        public static void ClearAll()
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return;
            }
            mgr.ClearAll();
        }

        public static FileDownloadJobInfo AddJob(FileManifest.FileItem file)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return null;
            }

            var desc = _CreateJobDesc(file);
            return mgr.AddJob(desc);
        }

        public static void SetCallBack(FileDownloadCallBack callback)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return;
            }
            mgr.SetCallBack(callback);
        }

        /// <summary>
        /// Pending -> pause, Downloading -> pause
        /// </summary>
        public static void Pause(FileDownloadJobInfo info)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return;
            }
            if (info == null)
            {
                FileDownloadLog.Assert(false, "param info is Null");
                return;
            }
            mgr.Pause(info.JobDesc.KeyName);
        }

        /// <summary>
        /// Pending -> pause, Downloading -> pause
        /// </summary>
        public static void Pause(string job_key_name)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return;
            }
           
            mgr.Pause(job_key_name);
        }

        /// <summary>
        /// Fail -> pending, Pause -> pending <br/>
        /// 有可能失效, 比如当前正在 Downloading, 先调用Pause, 还没有完全暂停, 再次调用 Restart        
        /// </summary>
        public static void Start(FileDownloadJobInfo info)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return;
            }
            if (info == null)
            {
                FileDownloadLog.Assert(false, "param info is Null");
                return;
            }
            mgr.Start(info.JobDesc.KeyName);
        }

        public static List<FileDownloadJobInfo> AddTasks(IList<FileManifest.FileItem> files)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return null;
            }

            if (files == null)
            {
                FileDownloadLog.Assert(false, "param files is Null");
                return null;
            }

            List<FileDownloadJobInfo> ret = new List<FileDownloadJobInfo>(files.Count);
            foreach (var file in files)
            {
                var info = mgr.AddJob(_CreateJobDesc(file));
                if (info != null)
                    ret.Add(info);
            }
            return ret;
        }

        private static FileDownloadJobDesc _CreateJobDesc(FileManifest.FileItem item)
        {
            string key_name = item.FullName;
            string remote_url = item.FullName;
            var ret = new FileDownloadJobDesc()
            {
                KeyName = item.FullName,
                RemoteUrl = DefaultServerUrl+item.FullName,
                Crc32 = item.Crc32,
                TotalSize = item.Size,
                UseGz = item.UseGz,        
                UserData = item,
                DestFilePath = DefaultSaveDir + item.FullName,
            };        

            return ret;
        }

        public static float GetProgress(this List<FileDownloadJobInfo> self, out long total_size, out long download_size)
        {
            total_size = 0;
            download_size = 0;
            if (self.Count == 0)
            {
                return 1.0f;
            }

            bool is_all_succ = false;
            foreach (var info in self)
            {
                total_size += info.TotalSize;

                if (info.Status == EFileDownloadStatus.Succ)
                    download_size += info.TotalSize;
                else
                {
                    is_all_succ = false;
                    download_size += info.DownloadSize;
                }
            }

            if (is_all_succ)
            {
                download_size = total_size;
                return 1.0f;
            }

            if (total_size <= 0)
            {
                return 0.99f;
            }
            float progress = (float)((double)download_size / (double)total_size);

            return System.Math.Clamp(progress, 0, 0.99f);
        }

        public static bool IsAllSucc(this List<FileDownloadJobInfo> self)
        {
            foreach (var p in self)
            {
                if (p.Status != EFileDownloadStatus.Succ)
                    return false;
            }
            return true;
        }


        public static FileDownloadJobInfo FindInfo(string file_full_name)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return null;
            }
            return mgr.FindInfo(file_full_name);
        }

        public static void GetAllInfo(List<FileDownloadJobInfo> out_list)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return;
            }
            mgr.GetAllJobs(out_list);
        }


        public static void Update()
        {
            _Inst.Val?.Update();
        }
    }

    public struct FileDownloadStat
    {
        public long TotalSize;
        public long DownloadedSize;

        public int SuccCount;
        public int FailedCount;
        public int DownloadingCount;
        public int PausedCount;

        public bool IsAllDone => DownloadingCount == 0 && PausedCount == 0;
        public int TotalCount => SuccCount + FailedCount + PausedCount + DownloadingCount;

        public float ProgressSize
        {
            get
            {
                if (DownloadingCount == 0 || PausedCount == 0)
                    return 1.0f;

                if (TotalSize <= 0)
                    return 0.99f;

                double ret = DownloadedSize / (double)TotalSize;
                return (float)Math.Clamp(ret, 0, 0.99);
            }
        }      
    
        public float ProgressCount
        {
            get
            {
                if (DownloadingCount == 0 || PausedCount == 0 )
                    return 1.0f;

                int total = TotalCount;
                int done_count = SuccCount + FailedCount;

                if (total <= 0)
                    return 1.0f;

                double ret = done_count / (double)total;
                return (float)Math.Clamp(ret, 0, 0.99);
            }
        }
    }

    public static class FileDownloadJobInfoExt
    {
        public static FileDownloadStat ExtGetSizeStat(this List<FileDownloadJobInfo> self)
        {
            FileDownloadStat ret = new();             
            if (self == null)
                return ret;

            foreach(var p in self)
            {
                if (p == null)
                    continue;
                ret.TotalSize +=p.TotalSize;
                ret.DownloadedSize += p.DownloadSize;

                switch (p.Status)
                {
                    case EFileDownloadStatus.Downloading:
                    case EFileDownloadStatus.Pending:
                        ret.DownloadingCount++;
                        break;
                    case EFileDownloadStatus.Succ:
                        ret.SuccCount++;
                        break;
                    case EFileDownloadStatus.Failed:
                        ret.FailedCount++;
                        break;
                    case EFileDownloadStatus.Pause:
                        ret.PausedCount++;
                        break;
                    default:
                        FileDownloadLog.E("unkown status {0}",p.Status);
                        break;
                } 
            }
            return ret;
        }      
    }
}
