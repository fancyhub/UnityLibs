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

        public Job AddJob(FileDownloadJobDesc job_desc)
        {
            if (!job_desc.IsValid())
            {
                FileDownloadLog.Assert(false, "param task is not valid");
                return null;
            }          

            _Dict.TryGetValue(job_desc.KeyName, out var job_node);
            if (job_node != null)
                return job_node.Value;

            Job ret = Job.Create(job_desc);

            if (System.IO.File.Exists(job_desc.DestFilePath))
            {
                FileDownloadLog.D("File {0} has Downloaded", job_desc.DestFilePath);
                ret.Status = EFileDownloadStatus.Succ;               
            }

            job_node = _StatusList[(int)ret.Status].ExtAddLast(ret);
            _Dict.Add(job_desc.KeyName, job_node);
            return ret;
        }

        public Job FindJob(string job_key_name)
        {
            if (string.IsNullOrEmpty(job_key_name))
            {
                FileDownloadLog.Assert(false, "param job_key_name is null");
                return null;
            }

            _Dict.TryGetValue(job_key_name, out var job_node);
            if (job_node != null)
            {
                return job_node.Value;
            }
            FileDownloadLog.Assert(false, "找不到 {0}", job_key_name);
            return null;
        }

        public void Change(Job job, EFileDownloadStatus status)
        {
            if (job == null || job._JobInfo == null)
            {
                FileDownloadLog.Assert(false, "param job is null, 代码有问题");
                return;
            }

            if (string.IsNullOrEmpty(job.JobKeyName))
            {
                FileDownloadLog.Assert(false, "param job.JobKeyName is null, 代码有问题");
                return;
            }

            _Dict.TryGetValue(job.JobKeyName, out var job_node);

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
                FileDownloadLog.Assert(false, "FileDownloadStatus 不能从 {0} -> {1}", job._JobInfo.Status, status);
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


        private static FsmStateTranMap<EFileDownloadStatus, EFileDownloadStatus> _JobStatusTranMap;
        private static bool _CanChangeStatus(EFileDownloadStatus from, EFileDownloadStatus to)
        {
            if (_JobStatusTranMap == null)
            {
                _JobStatusTranMap = new FsmStateTranMap<EFileDownloadStatus, EFileDownloadStatus>(EFileDownloadStatus.Pending);
                _JobStatusTranMap
                    .From(EFileDownloadStatus.Pending)
                        .To(EFileDownloadStatus.Downloading, EFileDownloadStatus.Downloading)
                        .To(EFileDownloadStatus.Pause, EFileDownloadStatus.Pause)
                    .From(EFileDownloadStatus.Downloading)
                        .To(EFileDownloadStatus.Pause, EFileDownloadStatus.Pause)
                        .To(EFileDownloadStatus.Succ, EFileDownloadStatus.Succ)
                        .To(EFileDownloadStatus.Failed, EFileDownloadStatus.Failed)
                    .From(EFileDownloadStatus.Pause)
                        .To(EFileDownloadStatus.Pending, EFileDownloadStatus.Pending)
                    .From(EFileDownloadStatus.Failed)
                        .To(EFileDownloadStatus.Pending, EFileDownloadStatus.Pending)
                    .From(EFileDownloadStatus.Succ);

            }
            return _JobStatusTranMap.Next(from, to, out var _);
        }
    }
}
