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

    public sealed class FileDownloadInfo
    {
        public readonly string Name;
        public readonly string FullName;
        public readonly long TotalSize;
        internal long _DownloadSize;
        internal EFileDownloadStatus _Status;

        internal FileDownloadInfo(string name, string full_name, long total_size, EFileDownloadStatus status)
        {
            this.Name = name;
            this.FullName = full_name;
            this.TotalSize = total_size;
            this._Status = status;
        }

        public EFileDownloadStatus Status => _Status;
        public long DownloadSize => _DownloadSize;


        public float Progress
        {
            get
            {
                if (_Status == EFileDownloadStatus.Succ)
                    return 1.0f;
                if (TotalSize <= 0)
                    return 0.99f;
                float p = (float)((double)_DownloadSize / (double)TotalSize);

                return System.Math.Clamp(p, 0, 0.99f);
            }
        }
    }

    public partial interface IFileDownloadMgr : ICPtr
    {
        public void Update();

        public FileDownloadInfo AddTask(FileManifest.FileItem file);

        /// <summary>
        /// Pending -> pause, Downloading -> pause
        /// </summary>
        public void Pause(string file_full_name);

        /// <summary>
        /// Fail -> pending, Pause -> pending <br/>
        /// 有可能失效, 比如当前正在 Downloading, 先调用Pause, 还没有完全暂停, 再次调用 Restart        
        /// </summary>
        public void Start(string file_full_name);

        public FileDownloadInfo FindInfo(string file_full_name);

        public void GetAllInfo(List<FileDownloadInfo> out_list);

        public void ClearAll();
    }

    public static class FileDownloadMgr
    {
        private static CPtr<IFileDownloadMgr> _Inst;

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

        public static FileDownloadInfo AddTask(FileManifest.FileItem file)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return null;
            }
            return mgr.AddTask(file);
        }

        /// <summary>
        /// Pending -> pause, Downloading -> pause
        /// </summary>
        public static void Pause(FileDownloadInfo info)
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
            mgr.Pause(info.FullName);
        }

        /// <summary>
        /// Fail -> pending, Pause -> pending <br/>
        /// 有可能失效, 比如当前正在 Downloading, 先调用Pause, 还没有完全暂停, 再次调用 Restart        
        /// </summary>
        public static void Start(FileDownloadInfo info)
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
            mgr.Start(info.FullName);
        }

        public static List<FileDownloadInfo> AddTasks(IList<FileManifest.FileItem> files)
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

            List<FileDownloadInfo> ret = new List<FileDownloadInfo>(files.Count);
            foreach (var file in files)
            {
                var info = mgr.AddTask(file);
                if (info != null)
                    ret.Add(info);
            }
            return ret;
        }

        public static float GetProgress(this List<FileDownloadInfo> self, out long total_size, out long download_size)
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

        public static bool IsAllSucc(this List<FileDownloadInfo> self)
        {
            foreach (var p in self)
            {
                if (p.Status != EFileDownloadStatus.Succ)
                    return false;
            }
            return true;
        }


        public static FileDownloadInfo FindInfo(string file_full_name)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return null;
            }
            return mgr.FindInfo(file_full_name);
        }

        public static void GetAllInfo(List<FileDownloadInfo> out_list)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return;
            }
            mgr.GetAllInfo(out_list);
        }


        public static void Update()
        {
            _Inst.Val?.Update();
        }
    }
}
