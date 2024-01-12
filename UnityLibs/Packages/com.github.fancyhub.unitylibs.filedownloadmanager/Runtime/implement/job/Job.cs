/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH.FileDownload
{
    public struct JobInfo
    {
        public string FullName;
        public uint Crc32;
        public bool UseGz;
    }

    internal sealed class Job : CPoolItemBase
    {
        public int _WorkerIndex = -1;
        public JobInfo _JobInfo;
        public FileDownloadInfo _DwonloadInfo;

        public static Job Create(FileManifest.FileItem file)
        {
            if (file == null)
                return null;
            var ret = GPool.New<Job>();
            ret._JobInfo.FullName = file.FullName;
            ret._JobInfo.Crc32 = file.Crc32;
            ret._JobInfo.UseGz = file.UseGz;
            ret._DwonloadInfo = new FileDownloadInfo(file.Name, file.FullName, file.Size, EFileDownloadStatus.Pending);
            return ret;
        }

        public EFileDownloadStatus Status
        {
            get
            {
                return _DwonloadInfo._Status;
            }
            set
            {
                _DwonloadInfo._Status = value;
            }
        }

        protected override void OnPoolRelease()
        {
            _WorkerIndex = -1;
            _DwonloadInfo = null;
        }
    }
}
