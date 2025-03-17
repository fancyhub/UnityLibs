/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FH.FileManagement
{
    internal sealed class FileMgrImplement : IFileMgr
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        private FileManifest _Manifest_Base;
        private FileManifest _Manifest_Local;
        private FileManifest _Manifest_Current;

        private FileCollection _FileCollection;

        private HashSet<string> _Tags = new HashSet<string>() { };

        private AndroidExtractStreamingAssetsOp _ExtractOp;

        public FileMgrImplement(IFileMgr.Config config)
        {
            _Tags.Clear();
            foreach (var p in config.BaseTags)
            {
                _Tags.Add(p);
            }

            _FileCollection = new FileCollection();
            _ExtractOp = new AndroidExtractStreamingAssetsOp(_FileCollection, config.AndroidExtractTag);
        }

        public void Init()
        {
            //1. 加载 Manifest
            _Manifest_Base = _LoadBaseManifest();
            if (_Manifest_Base == null && !Application.isEditor)
                FileLog._.E("加载本地{0} 失败", FileManifest.CDefaultFileName);

            _FileCollection.CollectStreamingAssets(_Manifest_Base);
            _FileCollection.CollectLocalDir();

            _Manifest_Local = _LoadLocalManifest();

            //2. 收集文件
            if (_Manifest_Local != null)
            {
                if (_IsAllTagsReady(_FileCollection, _Manifest_Local, true, _Tags))
                {
                    FileLog._.D("使用缓存里面的FileManifest,Version: {0}", _Manifest_Local.Version);
                    _Manifest_Current = _Manifest_Local;
                }
                else
                {
                    FileLog._.D("缓存里面的FileManifest有文件不存在, 所以不能使用");
                }
            }

            if (_Manifest_Current == null && _Manifest_Base != null)
            {
                List<FileManifest.FileItem> list = new();
                if (!_IsAllTagsReady(_FileCollection, _Manifest_Base, true, _Tags, list))
                {
                    FileLog._.E("Base Manifest, 有文件不存在, 打包出错了?");
                    for (int i = 0; i < list.Count; i++)
                    {
                        FileLog._.E("Base Manifest, 文件不存在 {0}/{1} : {2}", i + 1, list.Count, list[i].FullName);
                    }
                }

                _Manifest_Current = _Manifest_Base;
                FileLog._.D("使用StreamingAssets 里面的 FileManifest,Version: {0}", _Manifest_Base.Version);
            }

            _ExtractOp.StartAsync(_Manifest_Base);
        }

        public FileManifest GetCurrentManifest()
        {
            return _Manifest_Current;
        }

        public ExtractStreamingAssetsOperation GetExtractOperation()
        {
            return _ExtractOp;
        }

        public bool CopyFromStreamingAsset2CacheDir(string name)
        {
            if (_Manifest_Current == null)
            {
                FileLog._.D("Current FileManifest Is Null");
                return false;
            }

            var item = _Manifest_Current.FindFile(name);
            if (item == null)
            {
                FileLog._.D("找不到文件 {0}", name);
                return false;
            }

            _FileCollection.CollectLocalDir();
            return true;
        }

        public void Destroy()
        {
            ___ptr_ver++;
        }

        public VersionInfo GetVersionInfo()
        {
            if (_Manifest_Current == null)
                return default;
            return new VersionInfo(_Manifest_Current.Version);
        }

        public void RefreshFileList()
        {
            _FileCollection.CollectLocalDir();
        }

        /// <summary>
        /// 更新Manifest, true:更新成功, false:更新失败
        /// </summary>
        public bool Upgrade(FileManifest new_manifest, List<FileManifest.FileItem> out_need_download_list)
        {
            if (new_manifest == null)
            {
                FileLog._.Assert(false, "新的FileManifest 为空");
                return false;
            }

            _FileCollection.CollectLocalDir();
            if (!_IsAllTagsReady(_FileCollection, new_manifest, false, _Tags, out_need_download_list))
            {
                FileLog._.D("new FileManifest Is Not Ready");
                return false;
            }

            new_manifest.GetFilesWithTags(_Tags, _STempFileList);
            foreach (var p in _STempFileList)
            {
                if (string.IsNullOrEmpty(p.RelativePath))
                    continue;

                if (!_FileCollection.MoveFile2RelativeFile(p))
                {
                    FileLog._.E("切换的时候, 文件移动到 relative 目录失败 {0}", p.FullName);
                    return false;
                }
            }

            string path = FileSetting.LocalDir + FileManifest.CDefaultFileName;
            new_manifest.SaveTo(path);

            _Manifest_Local = new_manifest;
            _Manifest_Current = new_manifest;
            return true;
        }

        public void OnFileDownload(FileManifest.FileItem item)
        {
            if (item == null)
            {
                FileLog._.E("param item is null");
                return;
            }

            string file_full_path = FileSetting.LocalDir + item.FullName;
            if (!System.IO.File.Exists(file_full_path))
            {
                FileLog._.E("can't find the downloaded file: {0}", file_full_path);
                return;
            }

            _FileCollection.AddDownloadFile(item.FullName, file_full_path);

            if (string.IsNullOrEmpty(item.RelativePath))
                return;

            if (_Manifest_Current == null)
            {
                FileLog._.E("current FileManifest is null");
                return;
            }

            //是当前manifest需要的文件 && 是relative 文件类型, 需要移动
            if (_Manifest_Current.FindFile(item.Name) != null)
            {
                _FileCollection.MoveFile2RelativeFile(item);
            }
        }

        public bool IsAllTagsReady(FileManifest manifest, HashSet<string> tags = null, List<FileManifest.FileItem> out_need_download_list = null)
        {
            if (manifest == null)
            {
                FileLog._.Assert(false, "the param manifest is null");
                return false;
            }

            return _IsAllTagsReady(_FileCollection, manifest, manifest == _Manifest_Current, tags, out_need_download_list);
        }

        public bool IsAllFilesReady(FileManifest manifest, HashSet<string> file_names, List<FileManifest.FileItem> out_need_download_list = null)
        {
            if (manifest == null)
            {
                FileLog._.Assert(false, "the param manifest is null");
                return false;
            }

            _STempFileList.Clear();
            manifest.FindFiles(file_names, _STempFileList);
            return _IsAllFilesReady(_FileCollection, _STempFileList, manifest == _Manifest_Current, out_need_download_list);
        }

        public byte[] ReadAllBytes(string name)
        {
            var status = FindFile(name, out string full_path, out var file_location);
            if (status != EFileStatus.Ready || file_location == EFileLocation.None)
            {
                FileLog._.E("read file failed, Status:{0}, Location:{1}, {2}", status, file_location, name);
                return null;
            }

            if (file_location == EFileLocation.StreamingAssets)
            {
                FileLog._.D("read file from StreamingAssets : {0}->{1}", name, full_path);
                return SAFileSystem.ReadAllBytes(full_path);
            }
            else if (System.IO.File.Exists(full_path))
            {
                return System.IO.File.ReadAllBytes(full_path);
            }

            FileLog._.Assert(false, "internal error, file is not exist: {0}->{1}", name, full_path);
            return null;
        }

        public System.IO.Stream OpenRead(string name)
        {
            var status = FindFile(name, out string full_path, out var file_location);
            if (status != EFileStatus.Ready || file_location == EFileLocation.None)
            {
                FileLog._.E("read file failed:  Status:{0}, Location:{1}, {2}", status, file_location, name);
                return null;
            }

            if (file_location == EFileLocation.StreamingAssets)
            {
                FileLog._.D("从StreamingAssets 里面读取文件 {0}->{1}", name, full_path);
                return SAFileSystem.OpenRead(full_path);
            }
            else if (System.IO.File.Exists(full_path))
            {
                return System.IO.File.OpenRead(full_path);
            }

            FileLog._.Assert(false, "internal error, file is not exist: {0}->{1}", name, full_path);
            return null;
        }

        public EFileStatus FindFile(string name, out string full_path, out EFileLocation file_location)
        {
            if (_Manifest_Current == null)
            {
                FileLog._.D("Current FileManifest Is Null");
                full_path = null;
                file_location = EFileLocation.None;
                return EFileStatus.None;
            }

            var item = _Manifest_Current.FindFile(name);
            if (item == null)
            {
                FileLog._.D("can't find the file: {0}", name);
                full_path = null;
                file_location = EFileLocation.None;
                return EFileStatus.None;
            }

            return FindFile(item, out full_path, out file_location);
        }

        public EFileStatus FindFile(FileManifest.FileItem file, out string full_path, out EFileLocation file_location)
        {
            if (file == null)
            {
                FileLog._.D("Current FileManifest Is Null");
                full_path = null;
                file_location = EFileLocation.None;
                return EFileStatus.None;
            }

            (full_path, file_location) = _FileCollection.GetFullPath(file.FullName);
            if (file_location == EFileLocation.Remote)
                return EFileStatus.NeedDownload;
            return EFileStatus.Ready;
        }

        private FileManifest _LoadLocalManifest()
        {
            string file_path = FileSetting.LocalDir + FileManifest.CDefaultFileName;
            if (!System.IO.File.Exists(file_path))
            {
                FileLog._.D("缓存里面FileManifest不存在: {0}", file_path);
                return null;
            }
            string content = System.IO.File.ReadAllText(file_path);
            var ret = UnityEngine.JsonUtility.FromJson<FileManifest>(content);
            if (ret == null)
            {
                FileLog._.D("缓存里面的FileManifest加载失败: {0}", file_path);
            }
            return ret;
        }

        private FileManifest _LoadBaseManifest()
        {
            string file_path = FileSetting.StreamingAssetsDir + FileManifest.CDefaultFileName;
            byte[] bytes = FH.SAFileSystem.ReadAllBytes(file_path);
            if (bytes == null)
            {
                FileLog._.D("StreamingAssets 里面的FileManifest不存在: {0}", file_path);
                return null;
            }

            string content = System.Text.Encoding.UTF8.GetString(bytes);
            FileManifest ret = UnityEngine.JsonUtility.FromJson<FileManifest>(content);
            if (ret == null)
            {
                FileLog._.D("StreamingAssets里面的FileManifest加载失败: {0}", file_path);
            }
            return ret;
        }

        private static List<FileManifest.FileItem> _STempFileList = new List<FileManifest.FileItem>();

        /// <summary>
        /// 检查 manifest 里面有 含有 tag FileSetting.TagBase  的文件都存在
        /// </summary>        
        private static bool _IsAllTagsReady(
            FileCollection file_collection,
            FileManifest manifest,
            bool should_in_relative,
            HashSet<string> tags = null,
            List<FileManifest.FileItem> out_not_ready_list = null)
        {
            if (file_collection == null || manifest == null)
                return false;

            _STempFileList.Clear();
            if (tags == null)
                _STempFileList.AddRange(manifest.Files);
            else
                manifest.GetFilesWithTags(tags, _STempFileList);

            return _IsAllFilesReady(file_collection, _STempFileList, should_in_relative, out_not_ready_list);
        }

        private static bool _IsAllFilesReady(
            FileCollection file_collection,
            List<FileManifest.FileItem> file_items,
            bool should_in_relative,
            List<FileManifest.FileItem> out_not_ready_list = null)
        {
            if (file_collection == null || file_items == null)
                return false;

            if (out_not_ready_list == null)
            {
                bool ret = true;
                foreach (var p in file_items)
                {
                    if (file_collection.IsExist(p.FullName, should_in_relative, p.RelativePath))
                        continue;

                    FileLog._.D("文件 {0} 不存在", p.FullName);
                    ret = false;
#if !UNITY_EDITOR
                    break;
#endif
                }
                return ret;
            }
            else
            {
                out_not_ready_list.Clear();
                foreach (var p in file_items)
                {
                    if (file_collection.IsExist(p.FullName, should_in_relative, p.RelativePath))
                        continue;

                    FileLog._.D("文件 {0} 不存在", p.FullName);
                    out_not_ready_list.Add(p);
                }
                return out_not_ready_list.Count == 0;
            }
        }
    }
}