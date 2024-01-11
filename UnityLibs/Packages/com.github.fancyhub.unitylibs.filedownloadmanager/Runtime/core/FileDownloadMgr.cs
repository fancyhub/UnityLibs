/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
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

    public partial interface IFileDownloadMgr : ICPtr
    {
        public void Update();
        public void Download(FileManifest.FileItem file);
        public void Pause(FileManifest.FileItem file);
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

        public static void Download(FileManifest.FileItem file)
        {
            IFileDownloadMgr mgr = _Inst.Val;
            if (mgr == null)
            {
                FileDownloadLog.E("FileDownloadMgr is Null");
                return;
            }
            mgr.Download(file);
        }

        public static void Update()
        {
            _Inst.Val?.Update();
        }
    }
}
