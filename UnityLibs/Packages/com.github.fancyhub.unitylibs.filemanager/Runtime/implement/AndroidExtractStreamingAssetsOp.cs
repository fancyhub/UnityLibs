/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace FH.FileManagement
{
    internal sealed class AndroidExtractStreamingAssetsOp : ExtractStreamingAssetsOperation
    {
        private const string CVERSION_FILE_NAME = "base_res_version";

        private List<string> _FileList = new List<string>();
        private FileCollection _FileCollection;
        private string _Tag;
        private Thread _Thread;
        private int _TotalCount = 0;
        private int _DoneCount = 0;
        private string _Version;


        public AndroidExtractStreamingAssetsOp(FileCollection file_collection, string tag)
        {
            _FileCollection = file_collection;
            _Tag = tag;
        }
        #region 继承的
        public override bool IsDone => _TotalCount <= _DoneCount;

        public override float Progress
        {
            get
            {
                if (_TotalCount <= _DoneCount || _TotalCount == 0)
                    return 1;
                return (float)_DoneCount / _TotalCount;
            }
        }
        public override bool keepWaiting => _TotalCount > _DoneCount;
        #endregion

        public void StartAsync(FileManifest manifest)
        {
            if (_Thread != null)
                return;

            if (manifest == null || FileSetting.Platform != EFilePlatform.Android)
                return;

            _TotalCount = 0;
            _DoneCount = 0;
            string path = FileSetting.LocalDir + CVERSION_FILE_NAME;
            _Version = manifest.Version;
            if (_Version == null)
                _Version = "";
            if (System.IO.File.Exists(path))
            {
                string content = System.IO.File.ReadAllText(path);
                if (content == _Version)
                    return;
            }

            _FileList.Clear();
            foreach (var p in manifest.GetFilesWithTag(_Tag))
            {
                _FileList.Add(p.FullName);
            }
            _TotalCount = _FileList.Count;
            if (_TotalCount == 0)
                return;
            FileLog._.D("Begin Extra Streamign Assets {0}", _TotalCount);
            _Thread = new Thread(_CopyTask);
            _Thread.IsBackground = true;
            _Thread.Priority = ThreadPriority.Normal;
            _Thread.Start();
        }

        private void _CopyTask()
        {
            foreach (var p in _FileList)
            {
                FileLog._.D("Begin Extra Streamign Assets: {0}", p);
                string src_file_path = FileSetting.StreamingAssetsDir + p;
                string dest_file_path = FileSetting.LocalDir + p;

                if (System.IO.File.Exists(dest_file_path))
                    System.IO.File.Delete(dest_file_path);
                Stream fs_in = SAFileSystem.OpenRead(src_file_path);
                FileStream fs_out = File.Open(dest_file_path, FileMode.Create, FileAccess.Write);
                fs_in.CopyTo(fs_out);
                fs_out.Close();
                fs_in.Close();

                FileLog._.D("Done Extra Streamign Assets: {0}", p);
                _DoneCount++;
            }

            string path = FileSetting.LocalDir + CVERSION_FILE_NAME;
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            System.IO.File.WriteAllText(path, _Version);
            _FileCollection.CollectCacheDir();
        }
    }
}