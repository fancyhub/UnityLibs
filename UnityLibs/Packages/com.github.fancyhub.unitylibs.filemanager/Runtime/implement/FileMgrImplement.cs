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
            _Manifest_Local = _LoadLocalManifest();

            //2. 收集文件
            if (_Manifest_Local != null)
            {
                if (_IsAllReady(_FileCollection, _Manifest_Local, _Tags))
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
                if (!_IsAllReady(_FileCollection, _Manifest_Base, _Tags))
                    FileLog._.D("StreamingAssets的FileManifest有文件不存在");

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
            if (!_IsAllReady(_FileCollection, new_manifest, _Tags, out_need_download_list))
            {
                FileLog._.D("new FileManifest Is Not Ready");
                return false;
            }

            string path = FileSetting.LocalDir + FileManifest.CDefaultFileName;
            new_manifest.SaveTo(path);

            _Manifest_Local = new_manifest;
            _Manifest_Current = new_manifest;
            return true;
        }

        public bool IsAllReady(FileManifest manifest, HashSet<string> tags = null, List<FileManifest.FileItem> out_need_download_list = null)
        {
            if (manifest == null)
            {
                FileLog._.Assert(false, "新的FileManifest 为空");
                return false;
            }

            return _IsAllReady(_FileCollection, manifest, tags, out_need_download_list);
        }

        public byte[] ReadAllBytes(string name)
        {
            var status = FindFile(name, out string full_path);
            if (status != EFileStatus.Ready)
            {
                FileLog._.E("读取文件失败 {0} : {1}", status, name);
                return null;
            }

            if (full_path.StartsWith(FileSetting.StreamingAssetsDir))
            {
                FileLog._.D("从StreamingAssets 里面读取文件 {0}->{1}", name, full_path);
                return SAFileSystem.ReadAllBytes(full_path);
            }
            else if (System.IO.File.Exists(full_path))
            {
                return System.IO.File.ReadAllBytes(full_path);
            }

            FileLog._.Assert(false, "内部出错, 文件不存在 {0}->{1}", name, full_path);
            return null;
        }

        public System.IO.Stream OpenRead(string name)
        {
            var status = FindFile(name, out string full_path);
            if (status != EFileStatus.Ready)
            {
                FileLog._.E("读取文件失败 {0} : {1}", status, name);
                return null;
            }

            if (full_path.StartsWith(FileSetting.StreamingAssetsDir))
            {
                FileLog._.D("从StreamingAssets 里面读取文件 {0}->{1}", name, full_path);
                return SAFileSystem.OpenRead(full_path);
            }
            else if (System.IO.File.Exists(full_path))
            {
                return System.IO.File.OpenRead(full_path);
            }

            FileLog._.Assert(false, "内部出错, 文件不存在 {0}->{1}", name, full_path);
            return null;
        }

        public EFileStatus FindFile(string name, out string full_path)
        {
            if (_Manifest_Current == null)
            {
                FileLog._.D("Current FileManifest Is Null");
                full_path = null;
                return EFileStatus.None;
            }

            var item = _Manifest_Current.FindFile(name);
            if (item == null)
            {
                FileLog._.D("找不到文件 {0}", name);
                full_path = null;
                return EFileStatus.None;
            }

            full_path = _FileCollection.GetFullPath(item.FullName);
            if (full_path == null)
                return EFileStatus.Remote;
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
        private static bool _IsAllReady(
            FileCollection file_collection,
            FileManifest manifest,
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

            if (out_not_ready_list == null)
            {
                bool ret = true;
                foreach (var p in _STempFileList)
                {
                    if (file_collection.GetFullPath(p.FullName) != null)
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
                foreach (var p in _STempFileList)
                {
                    if (file_collection.GetFullPath(p.FullName) != null)
                        continue;

                    FileLog._.D("文件 {0} 不存在", p.FullName);
                    out_not_ready_list.Add(p);
                }
                return out_not_ready_list.Count == 0;
            }
        }
    }
}