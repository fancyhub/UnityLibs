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
        public struct FileInfo
        {
            public readonly string FullName;
            public readonly string FullPath;
            public readonly string RelativePath;

            public FileInfo(string full_name, string full_path)
            {
                FullName = full_name;
                FullPath = full_path;
                RelativePath = null;
            }

            public FileInfo(string full_name, string full_path, string relative_path)
            {
                FullName = full_name;
                FullPath = full_path;
                RelativePath = relative_path;
            }
        }

        private struct TempRelativeFileInfo
        {
            public string OrigFilePath;
            public string FullNameFilePath;
        }

        private readonly string _LocalDir;
        private readonly string _LocalRelativeFileDir;

        //Key: full_name, Value: full_path
        private Dictionary<string, string> _FilesInStreamingAssets = new();

        //key: full_name
        private Dictionary<string, FileInfo> _FilesInLocalDir = new();

        //key: relative path
        private Dictionary<string, FileInfo> _RelativeFilesInLocalDir = new();

        private Dictionary<string, TempRelativeFileInfo> _TempRelativeFileInfo = new();

        public FileCollection()
        {
            _LocalDir = FileSetting.LocalDir;
            _LocalRelativeFileDir = FileSetting.LocalRelativeFileDir;
        }

        //只在主线程执行
        public void CollectLocalDir()
        {
            //1. 清除
            _FilesInLocalDir.Clear();

            //2.collect normal files
            string[] files = System.IO.Directory.GetFiles(_LocalDir, "*.*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var full_path in files)
            {
                string file_name = System.IO.Path.GetFileName(full_path);
                _FilesInLocalDir[file_name] = new(file_name, full_path);
            }

            //3. collect relative files
            files = System.IO.Directory.GetFiles(_LocalRelativeFileDir, "*.*", System.IO.SearchOption.AllDirectories);
            _TempRelativeFileInfo.Clear();
            foreach (var full_path in files)
            {
                string relative_path = full_path.Substring(_LocalRelativeFileDir.Length);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                relative_path = relative_path.Replace('\\', '/');
#endif
                if (relative_path.EndsWith(FileSetting.CRelativeFileFullNameExt))
                {
                    relative_path = relative_path.Substring(0, relative_path.Length - FileSetting.CRelativeFileFullNameExt.Length);

                    _TempRelativeFileInfo.TryGetValue(relative_path, out var info);
                    info.FullNameFilePath = full_path;
                    _TempRelativeFileInfo[relative_path] = info;
                }
                else
                {
                    _TempRelativeFileInfo.TryGetValue(relative_path, out var info);
                    info.OrigFilePath = full_path;
                    _TempRelativeFileInfo[relative_path] = info;
                }
            }

            foreach (var p in _TempRelativeFileInfo)
            {
                var info = p.Value;
                if (info.OrigFilePath != null && info.FullNameFilePath != null)
                {
                    string full_name = System.IO.File.ReadAllText(info.FullNameFilePath);

                    FileInfo file_info = new(full_name, info.OrigFilePath, p.Key);
                    _FilesInLocalDir[full_name] = file_info;
                    _RelativeFilesInLocalDir[p.Key] = file_info;
                }
                else
                {
                    try
                    {
                        if (info.OrigFilePath != null)
                            System.IO.File.Delete(info.OrigFilePath);
                        if (info.FullNameFilePath != null)
                            System.IO.File.Delete(info.FullNameFilePath);
                    }
                    catch (System.Exception e)
                    {
                        FileLog._.E(e);
                    }
                }
            }
            _TempRelativeFileInfo.Clear();
        }

        //只在主线程执行
        public void AddDownloadFile(string full_name, string full_path)
        {
            _FilesInLocalDir[full_name] = new(full_name, full_path);
        }

        /// <summary>
        /// 把文件移动到相对路径
        /// </summary>
        public bool MoveFile2RelativeFile(FileManifest.FileItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.FullName) || string.IsNullOrEmpty(item.RelativePath))
                return false;

            //当前该文件已经处于 relative 模式了
            if (_RelativeFilesInLocalDir.TryGetValue(item.RelativePath, out var info) && info.FullName == item.FullName)
                return true;

            string src_file_path = _LocalDir + item.FullName;
            if (!System.IO.File.Exists(src_file_path))
                return false;


            string dest_file_path = _LocalRelativeFileDir + item.RelativePath;
            FileLog._.D("Move file: {0} -> {1}", src_file_path, dest_file_path);

            //先移动当前的文件
            if (!_MoveRelativeFileBack(item.RelativePath))
                return false;

            //移动到相对目录
            try
            {
                FileUtil.CreateFileDir(dest_file_path);
                System.IO.File.Move(src_file_path, dest_file_path);
                System.IO.File.WriteAllText(dest_file_path + FileSetting.CRelativeFileFullNameExt, item.FullName);

                info = new FileInfo(item.FullName, dest_file_path, item.RelativePath);
                _FilesInLocalDir[item.FullName] = info;
                _RelativeFilesInLocalDir[item.RelativePath] = info;
                return true;
            }
            catch (System.Exception e)
            {
                FileLog._.E(e);
                return false;
            }
        }

        public (string, EFileLocation) GetFullPath(string full_name)
        {
            if (string.IsNullOrEmpty(full_name))
            {
                FileLog._.D("FileManifest.FileItem Is Null");
                return (null, EFileLocation.None);
            }

            if (_FilesInLocalDir.TryGetValue(full_name, out var info))
                return (info.FullPath, EFileLocation.Persistent);

            if (_FilesInStreamingAssets.TryGetValue(full_name, out var path))
                return (path, EFileLocation.StreamingAssets);

            return (null, EFileLocation.Remote);
        }

        public bool IsExist(string full_name, bool should_in_retaive, string relative_path)
        {
            if (string.IsNullOrEmpty(full_name))
                return false;

            if (_FilesInLocalDir.TryGetValue(full_name, out var info))
            {
                if (!should_in_retaive || string.IsNullOrEmpty(relative_path))
                    return true;
                else if (info.RelativePath == relative_path)
                    return true;
            }

            if (_FilesInStreamingAssets.ContainsKey(full_name))
                return true;
            return false;
        }

        public void CollectStreamingAssets(FileManifest base_manifest)
        {
            //1. check
            if (base_manifest == null)
                return;

            //2. 清除
            _FilesInStreamingAssets.Clear();

            //3. collect normal files
            List<string> files = new List<string>();
            FH.SAFileSystem.GetFileList(FileSetting.StreamingAssetsDir, false, files);
            if (files.Count == 0)
                FileLog._.D("StreamingAssets文件为空: {0}", FileSetting.StreamingAssetsDir);
            foreach (var full_path in files)
            {
                string file_name = System.IO.Path.GetFileName(full_path);
                _FilesInStreamingAssets.Add(file_name, full_path);
            }

            //4. collect relative files 
            files.Clear();
            FH.SAFileSystem.GetFileList(FileSetting.StreamingAssetsRelativeFileDir, true, files);
            foreach (var full_path in files)
            {
                string relative_file_path = full_path.Substring(FileSetting.StreamingAssetsRelativeFileDir.Length);

                var item = base_manifest.FindFileByRelativePath(relative_file_path);
                if (item == null)
                {
                    FileLog._.E("can't find the file in the base manifest: {0}", full_path);
                    continue;
                }

                _FilesInStreamingAssets.Add(item.FullName, full_path);
                FileLog._.D("StreamingAssets Collect: {0} -> {1}", item.FullName, full_path);
            }
        }


        //把当前relative文件移走, 为了后续把新文件移动到该目录
        private bool _MoveRelativeFileBack(string relative_file_path)
        {
            if (string.IsNullOrEmpty(relative_file_path))
                return false;

            bool exist_in_dict = _RelativeFilesInLocalDir.TryGetValue(relative_file_path, out var info);
            string src_file_path = _LocalRelativeFileDir + relative_file_path;
            if (!exist_in_dict)
            {
                if (!System.IO.File.Exists(src_file_path))
                    return true;

                //数据出问题了
                FileLog._.E("relative file is not in the dict : {0}, delete file: {1} ", relative_file_path, src_file_path);
                try
                {
                    System.IO.File.Delete(src_file_path);
                    return true;
                }
                catch (System.Exception e)
                {
                    FileLog._.E("delete failed: {0}", src_file_path);
                    FileLog._.E(e);
                    return false;
                }
            }

            _RelativeFilesInLocalDir.Remove(relative_file_path);

            //文件不存在
            if (!System.IO.File.Exists(src_file_path))
            {
                FileLog._.E("file is delete in some where : {0}", info.FullName, src_file_path);
                _FilesInLocalDir.Remove(info.FullName);
                return true;
            }


            //目标位置已经有文件了
            string dest_file_path = _LocalDir + info.FullName;
            if (System.IO.File.Exists(dest_file_path))
            {
                FileLog._.E("file duplicate: {0}, {1}, delete file: {2}", info.FullName, dest_file_path, src_file_path);
                //直接删除本地文件
                try
                {
                    System.IO.File.Delete(src_file_path);
                    _FilesInLocalDir[info.FullName] = new(info.FullName, dest_file_path); //更新文件列表
                    return true;
                }
                catch (System.Exception e)
                {
                    FileLog._.E("delete failed: {0}", src_file_path);
                    FileLog._.E(e);
                    return false;
                }
            }

            //移动文件
            FileLog._.D("move file: {0}, {1} -> {2}", info.FullName, src_file_path, dest_file_path);
            try
            {
                System.IO.File.Move(src_file_path, dest_file_path); //移动到非相对路径
                _FilesInLocalDir[info.FullName] = new FileInfo(info.FullName, dest_file_path);
                return true;
            }
            catch (System.Exception e)
            {
                FileLog._.E(e);
                return false;
            }
        }
    }
}