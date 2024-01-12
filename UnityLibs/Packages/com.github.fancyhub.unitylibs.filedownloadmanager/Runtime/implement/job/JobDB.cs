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
        public LinkedList<Job>[] _StatusList;

        public JobDB()
        {
            _Dict = new();
            _StatusList = new LinkedList<Job>[1 + (int)EFileDownloadStatus.Succ];
            _StatusList[(int)EFileDownloadStatus.Pending] = new LinkedList<Job>();
            _StatusList[(int)EFileDownloadStatus.Downloading] = new LinkedList<Job>();
            _StatusList[(int)EFileDownloadStatus.Pause] = new LinkedList<Job>();
            _StatusList[(int)EFileDownloadStatus.Failed] = new LinkedList<Job>();
            _StatusList[(int)EFileDownloadStatus.Succ] = new LinkedList<Job>();

        }

        public void ClearAll()
        {
            foreach (var p in _Dict)
            {
                p.Value.Value.Destroy();
            }
            _Dict.Clear();

            foreach (var p in _StatusList)
            {
                p.ExtClear();
            }
        }

        public Job AddJob(FileManifest.FileItem file)
        {
            if (file == null)
            {
                FileDownloadLog.Assert(false, "param file is null");
                return null;
            }
            if (file.FullName == null)
            {
                FileDownloadLog.Assert(false, "param file.FullName is null");
                return null;
            }

            _Dict.TryGetValue(file.FullName, out var job_node);
            if (job_node != null)
                return job_node.Value;

            Job ret = Job.Create(file);
            job_node = _StatusList[(int)ret.Status].ExtAddLast(ret);
            _Dict.Add(file.FullName, job_node);
            return ret;
        }

        public Job FindJob(string file_full_name)
        {
            if (string.IsNullOrEmpty(file_full_name))
            {
                FileDownloadLog.Assert(false, "param file_full_name is null");
                return null;
            }

            _Dict.TryGetValue(file_full_name, out var job_node);
            if (job_node != null)
            {
                return job_node.Value;
            }
            FileDownloadLog.Assert(false, "找不到 {0}", file_full_name);
            return null;
        }

        public void Change(Job job, EFileDownloadStatus status)
        {
            if (job == null || job._DwonloadInfo == null)
            {
                FileDownloadLog.Assert(false, "param job is null, 代码有问题");
                return;
            }

            if (string.IsNullOrEmpty(job._JobInfo.FullName))
            {
                FileDownloadLog.Assert(false, "param job.JobInfo.FullName is null, 代码有问题");
                return;
            }

            _Dict.TryGetValue(job._JobInfo.FullName, out var job_node);
            if (job_node == null)
            {
                FileDownloadLog.Assert(false, "内部错误, 代码有问题");
                return;
            }

            if (job_node.Value != job)
            {
                FileDownloadLog.Assert(false, "内部错误, 代码有问题");
                return;
            }

            if (job.Status == status)
                return;

            //状态检查
            if (!_CanChangeStatus(job.Status, status))
            {
                FileDownloadLog.Assert(false, "FileDownloadStatus 不能从 {0} -> {1}", job._DwonloadInfo.Status, status);
                return;
            }
            job.Status = status;

            var new_list = _StatusList[(int)status];
            job_node.List.Remove(job_node);
            new_list.AddLast(job_node);
        }

        public Job PopPending()
        {
            var pending_list = _StatusList[(int)EFileDownloadStatus.Pending];

            if (pending_list.Count == 0)
                return null;
            var node = pending_list.First;
            var ret = node.Value;
            pending_list.Remove(node);

            ret.Status = EFileDownloadStatus.Downloading;
            var download_list = _StatusList[(int)EFileDownloadStatus.Downloading];
            download_list.AddLast(node);
            return ret;
        }


        private static StateTranMap<EFileDownloadStatus, EFileDownloadStatus> _JobStatusTranMap;
        private static bool _CanChangeStatus(EFileDownloadStatus from, EFileDownloadStatus to)
        {
            if (_JobStatusTranMap == null)
            {
                _JobStatusTranMap = new StateTranMap<EFileDownloadStatus, EFileDownloadStatus>(EFileDownloadStatus.Pending);
                _JobStatusTranMap
                    .Begin(EFileDownloadStatus.Pending)
                        .Add(EFileDownloadStatus.Downloading, EFileDownloadStatus.Downloading)
                        .Add(EFileDownloadStatus.Pause, EFileDownloadStatus.Pause)
                    .Begin(EFileDownloadStatus.Downloading)
                        .Add(EFileDownloadStatus.Pause, EFileDownloadStatus.Pause)
                        .Add(EFileDownloadStatus.Succ, EFileDownloadStatus.Succ)
                        .Add(EFileDownloadStatus.Failed, EFileDownloadStatus.Failed)
                    .Begin(EFileDownloadStatus.Pause)
                        .Add(EFileDownloadStatus.Pending, EFileDownloadStatus.Pending)
                    .Begin(EFileDownloadStatus.Failed)
                        .Add(EFileDownloadStatus.Pending, EFileDownloadStatus.Pending)
                    .Begin(EFileDownloadStatus.Succ);

            }
            return _JobStatusTranMap.Next(from, to, out var _);             
        }
    }
}
