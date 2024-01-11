/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.FileDownload
{
    internal sealed class JobDB
    {
        public Dictionary<string, LinkedListNode<Job>> _Dict;

        public LinkedList<Job> _Pending;
        public LinkedList<Job> _Paused;
        public LinkedList<Job> _Running;
        public LinkedList<Job> _Succ;
        public LinkedList<Job> _Fail;

        public JobDB()
        {
            _Dict = new();
            _Pending = new LinkedList<Job>();
            _Paused = new LinkedList<Job>();
            _Running = new LinkedList<Job>();
            _Succ = new LinkedList<Job>();
            _Fail = new LinkedList<Job>();
        }

        public void ClearAll()
        {
            foreach(var p in _Dict)
            {
                p.Value.Value.Destroy();
            }
            _Dict.Clear();
            _Pending.ExtClear();
            _Paused.ExtClear();
            _Running.ExtClear();
            _Succ.ExtClear();
            _Fail.ExtClear();
        }

        public Job AddJob(FileManifest.FileItem file)
        {
            if (file == null)
            {
                FileDownloadLog.E("param file is null");
                return null;
            }
            if (file.FullName == null)
            {
                FileDownloadLog.E("param file.FullName is null");
                return null;
            }

            _Dict.TryGetValue(file.FullName, out var job_node);
            if (job_node != null)
                return job_node.Value;

            Job ret = Job.Create(file);
            job_node = _Pending.ExtAddLast(ret);
            _Dict.Add(file.FullName, job_node);
            return ret;
        }

        public Job FindJob(FileManifest.FileItem file)
        {
            if (file == null || file.FullName == null)
                return null;

            _Dict.TryGetValue(file.FullName, out var job_node);
            if (job_node != null)
                return job_node.Value;
            return null;
        }

        public void Change(Job job, EFileDownloadStatus status)
        {
            if (job == null || job.File == null)
                return;

            _Dict.TryGetValue(job.File.FullName, out var job_node);
            if (job_node == null)
            {
                FileDownloadLog.Assert(false, "内部错误, 代码有问题");
                //Error
                return;
            }

            if (job_node.Value != job)
            {
                FileDownloadLog.Assert(false, "内部错误, 代码有问题");
                //Error
                return;
            }

            if (job.Status == status)
                return;
            //状态检查
            if (!_CanChangeStatus(job.Status, status))
            {
                FileDownloadLog.Assert(false, "FileDownloadStatus 不能从 {0} -> {1}", job.Status, status);
                return;
            }
            job.Status = status;

            switch (job.Status)
            {
                case EFileDownloadStatus.Pause:
                    job_node.List.Remove(job_node);
                    _Paused.AddLast(job_node);
                    break;
                case EFileDownloadStatus.Succ:
                    job_node.List.Remove(job_node);
                    _Succ.AddLast(job_node);
                    break;
                case EFileDownloadStatus.Failed:
                    job_node.List.Remove(job_node);
                    _Fail.AddLast(job_node);
                    break;
                case EFileDownloadStatus.Pending:
                    job_node.List.Remove(job_node);
                    _Pending.AddLast(job_node);
                    break;
                case EFileDownloadStatus.Downloading:
                    job_node.List.Remove(job_node);
                    _Running.AddLast(job_node);
                    break;
            }
        }

        public Job PopPending()
        {
            if (_Pending.Count == 0)
                return null;
            var node = _Pending.First;
            var ret = node.Value;
            _Pending.Remove(node);

            ret.Status = EFileDownloadStatus.Downloading;
            _Running.AddLast(node);
            return ret;
        }

        private static bool _CanChangeStatus(EFileDownloadStatus from, EFileDownloadStatus to)
        {
            switch (from)
            {
                case EFileDownloadStatus.Pending: //等待,排队中
                    if (to == EFileDownloadStatus.Pause || to == EFileDownloadStatus.Downloading)
                        return true;
                    return false;

                case EFileDownloadStatus.Downloading: //正在下载
                    if (to == EFileDownloadStatus.Pending)
                        return false;
                    return true;

                case EFileDownloadStatus.Pause:
                    if (to == EFileDownloadStatus.Pending)
                        return true;
                    return false;

                case EFileDownloadStatus.Failed: //下载失败
                    if (to == EFileDownloadStatus.Pending)
                        return true;
                    return false;
                case EFileDownloadStatus.Succ: //下载成功
                    return false;
                default: return false;
            }
        }
    }
}
