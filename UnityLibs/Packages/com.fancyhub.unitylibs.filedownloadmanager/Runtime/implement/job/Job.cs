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
        public int _WorkerIndex = -1;       
        public FileDownloadJobInfo _JobInfo;

        public static Job Create(FileDownloadJobDesc desc)
        {        
            var ret = GPool.New<Job>();           
            ret._JobInfo = new FileDownloadJobInfo(desc);
            ret._JobInfo._Status = EFileDownloadStatus.Pending;      
            
            return ret;
        }

        public EFileDownloadStatus Status
        {
            get
            {
                if(_JobInfo!=null)
                    return _JobInfo._Status;
                return EFileDownloadStatus.Failed;
            }
            set
            {
                if(_JobInfo!=null)
                    _JobInfo._Status = value;
            }
        }

        public string JobKeyName
        {
            get
            {
                if (_JobInfo == null)
                    return "";
                return _JobInfo.JobDesc.KeyName;
            }
        }

        protected override void OnPoolRelease()
        {
            _WorkerIndex = -1;
            _JobInfo = null;
        }
    }
}
