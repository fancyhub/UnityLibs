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
                if (DownloadingCount == 0 && PausedCount == 0)
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
                if (DownloadingCount == 0 && PausedCount == 0)
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

            foreach (var p in self)
            {
                if (p == null)
                    continue;
                ret.TotalSize += p.TotalSize;
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
                        FileDownloadLog.E("unkown status {0}", p.Status);
                        break;
                }
            }
            return ret;
        }

        public static float ExtGetProgress(this List<FileDownloadJobInfo> self, out long total_size, out long download_size)
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

        public static bool ExtIsAllSucc(this List<FileDownloadJobInfo> self)
        {
            foreach (var p in self)
            {
                if (p.Status != EFileDownloadStatus.Succ)
                    return false;
            }
            return true;
        }

    }
}
