/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace FH.FileManagement
{
    internal sealed class FileCollection
    {
        //Key: file_name, Value: file_path
        private Dictionary<string, string> _FilesInStreamingAssets = new();
        private string _CacheDir;

        public FileCollection()
        {
            _CacheDir = FileSetting.CacheDir;
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
                string file_path = System.IO.Path.Combine(Application.streamingAssetsPath, f);
                file_path = file_path.Replace('\\', '/');
                _FilesInStreamingAssets.Add(file_name, file_path);

                FileLog._.D("StreamingAssets Collect {0} -> {1}", file_name, file_path);
            }

           
        }

        public bool IsExist(string file_name)
        {
            if (string.IsNullOrEmpty(file_name))
                return false;

            if (_FilesInStreamingAssets.ContainsKey(file_name))
                return true;

            string full_path = _CacheDir + file_name;
            return System.IO.File.Exists(full_path);
        }

        public string GetFullPath(string file_name)
        {
            if (string.IsNullOrEmpty(file_name))
                return null;

            if (_FilesInStreamingAssets.TryGetValue(file_name, out string full_path))
                return full_path;

            return _CacheDir + file_name;
        }
    }
}