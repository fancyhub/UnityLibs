/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH.FileDownload
{
    internal sealed class Job : CPoolItemBase
    {
        public EFileDownloadStatus Status = EFileDownloadStatus.Pending;
        public FileManifest.FileItem File;
        public int WorkerIndex = -1;
        public long DownloadedSize;

        public static Job Create(FileManifest.FileItem file)
        {
            if (file == null)
                return null;
            var ret = GPool.New<Job>();
            ret.File = file;
            return ret;
        }

        protected override void OnPoolRelease()
        {
            WorkerIndex = -1;
            File = null;
            Status = EFileDownloadStatus.Pending;            
            DownloadedSize = 0;
        }
    }
}
