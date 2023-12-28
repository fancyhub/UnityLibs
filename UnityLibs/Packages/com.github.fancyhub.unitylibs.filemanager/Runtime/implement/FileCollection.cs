/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;

namespace FH.FileManagement
{
    internal sealed class FileCollection
    {
        //Key: file_name, Value: full_path
        private Dictionary<string, string> _FilesInStreamingAssets = new();
        private Dictionary<string, string> _FilesInCache = new();

        private readonly string _CacheDir;

        public FileCollection()
        {
            _CacheDir = FileSetting.LocalDir;
        }

        public void CollectCacheDir()
        {
            Dictionary<string, string> new_Dict = new Dictionary<string, string>();
            string[] files = System.IO.Directory.GetFiles(_CacheDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var p in files)
            {
                string file_name = System.IO.Path.GetFileName(p);
                new_Dict[file_name] = p;
            }
            _FilesInCache = new_Dict;
        }

        public void CollectStreamingAssets()
        {
            List<string> files = new List<string>();
            FH.SAFileSystem.GetFileList(FileSetting.StreamingAssetsDir, false, files);
            if (files.Count == 0)
            {
                FileLog._.D("StreamingAssets文件为空 {0}", FileSetting.StreamingAssetsDir);
            }

            _FilesInStreamingAssets.Clear();
            foreach (var f in files)
            {
                string file_name = System.IO.Path.GetFileName(f);
                _FilesInStreamingAssets.Add(file_name, f);

                FileLog._.D("StreamingAssets Collect {0} -> {1}", file_name, f);
            }
        }

        public bool IsExist(string file_name)
        {
            if (string.IsNullOrEmpty(file_name))
                return false;

            if (_FilesInCache.ContainsKey(file_name))
                return true;
            if (_FilesInStreamingAssets.ContainsKey(file_name))
                return true;
            return false;
        }

        public string GetFullPath(string file_name)
        {
            if (string.IsNullOrEmpty(file_name))
                return null;

            if (_FilesInCache.TryGetValue(file_name, out var path))
                return path;
            if (_FilesInStreamingAssets.TryGetValue(file_name, out path))
                return path;
            return null;
        }
    }
}