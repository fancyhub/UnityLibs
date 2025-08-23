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
        private List<FileManifest.FileItem> _FileList = new List<FileManifest.FileItem>();
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
            string path = FileSetting.LocalDir + FileSetting.CBaseResVersionFileName;
            _Version = manifest.Version;
            if (_Version == null)
                _Version = "";
            if (System.IO.File.Exists(path))
            {
                string content = System.IO.File.ReadAllText(path);
                if (content == _Version)
                {
                    FileLog._.I("Don't need Extra Android Streamign Assets, version is same: {0}", _Version);
                    return;
                }
            }

            _FileList.Clear();
            _FileList.AddRange(manifest.GetFilesWithTag(_Tag));
            _TotalCount = _FileList.Count;
            if (_TotalCount == 0)
                return;
            FileLog._.I("Begin Extra Android Streamign Assets {0}", _TotalCount);
            _Thread = new Thread(_CopyTask);
            _Thread.IsBackground = true;
            _Thread.Priority = ThreadPriority.Highest;
            _Thread.Start();

            UnityMono.StartCheck(this, _FileCollection, _Version);
        }


        public class UnityMono : UnityEngine.MonoBehaviour
        {
            public static void StartCheck(ExtractStreamingAssetsOperation operation, FileCollection file_collection, string version)
            {
                UnityEngine.GameObject obj = new UnityEngine.GameObject("");
                obj.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
                UnityEngine.GameObject.DontDestroyOnLoad(obj);
                var comp = obj.AddComponent<UnityMono>();
                comp.StartCoroutine(_WaitTaskDone(operation, file_collection, version, obj));
            }

            private static System.Collections.IEnumerator _WaitTaskDone(ExtractStreamingAssetsOperation operation, FileCollection file_collection, string version, UnityEngine.GameObject obj)
            {
                yield return operation;

                string path = FileSetting.LocalDir + FileSetting.CBaseResVersionFileName;
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                System.IO.File.WriteAllText(path, version);
                FileLog._.I("End Extra Android Streamign Assets");
                file_collection.CollectLocalDir();
                UnityEngine.GameObject.Destroy(obj);
            }
        }

        private void _CopyTask()
        {
            foreach (var p in _FileList)
            {
                FileLog._.D("Begin Extra Streaming Asset: {0}", p.FullName);
                string src_file_path = FileSetting.StreamingAssetsDir;
                string dest_file_path = FileSetting.LocalDir;
                bool is_relative_file = false;
                if (string.IsNullOrEmpty(p.RelativePath))
                {
                    src_file_path = FileSetting.StreamingAssetsDir + p.FullName;
                    dest_file_path = FileSetting.LocalDir + p.FullName;
                    is_relative_file = false;
                }
                else
                {
                    src_file_path = FileSetting.StreamingAssetsRelativeFileDir + p.RelativePath;
                    dest_file_path = FileSetting.LocalRelativeFileDir + p.RelativePath;
                    is_relative_file = true;
                }

                if (System.IO.File.Exists(dest_file_path))
                    System.IO.File.Delete(dest_file_path);
                if (is_relative_file)
                    FileUtil.CreateFileDir(dest_file_path);
                Stream fs_in = SAFileSystem.OpenRead(src_file_path);
                if (fs_in == null)
                {
                    FileLog._.E("Extra streaming asset failed, file is not exist: {0}", src_file_path);
                }
                else
                {
                    FileStream fs_out = File.Open(dest_file_path, FileMode.Create, FileAccess.Write);
                    fs_in.CopyTo(fs_out);
                    fs_out.Close();
                    fs_in.Close();
                    if (is_relative_file)
                        File.WriteAllText(dest_file_path + FileSetting.CRelativeFileFullNameExt, p.FullName);
                    FileLog._.D("Done Extra Streaming Asset: {0}", p.FullName);
                }
                _DoneCount++;
            }
        }
    }
}