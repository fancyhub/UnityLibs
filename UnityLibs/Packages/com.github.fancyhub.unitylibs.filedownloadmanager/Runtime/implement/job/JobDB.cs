
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
                //Error
                return;
            }

            if (job_node.Value != job)
            {
                //Error
                return;
            }

            if (job.Status == status)
                return;
            //状态检查

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
    }
}
