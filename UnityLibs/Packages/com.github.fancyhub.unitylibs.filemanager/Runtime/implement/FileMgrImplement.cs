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
    internal sealed class FileMgrImplement : IFileMgr
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        private FileManifest _Manifest_Base;
        private FileManifest _Manifest_Local;
        private FileManifest _Manifest_Current;

        private FileCollection _FileCollection;

        private HashSet<string> _Tags = new HashSet<string>() { FileSetting.TagBase };

        public FileMgrImplement()
        {
            //1. 加载 Manifest
            _Manifest_Base = _LoadBaseManifest();
            if (_Manifest_Base == null && !Application.isEditor)
                FileLog._.E("加载本地{0} 失败", FileSetting.ManifestName);
            _Manifest_Local = _LoadLocalManifest();

            //2. 收集文件
            _FileCollection = new FileCollection();
            _FileCollection.CollectStreamingAssets();

            if (_Manifest_Local != null)
            {
                if (_CheckFileExists(_FileCollection, _Manifest_Local, _Tags))
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
                if (_CheckFileExists(_FileCollection, _Manifest_Base, _Tags))
                {
                    FileLog._.D("使用StreamingAssets 里面的 FileManifest,Version: {0}", _Manifest_Base.Version);
                    _Manifest_Current = _Manifest_Base;

                }
                else
                {
                    FileLog._.D("StreamingAssets的FileManifest有文件不存在, 所以不能使用");
                }
            }
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
        public bool UpgradeManifest()
        {
            FileManifest new_manifest = _LoadUpgradeManifest();
            if (new_manifest == null)
            {
                FileLog._.D("新的FileManifest 为空");
                return false;
            }

            if (!_CheckFileExists(_FileCollection, new_manifest, _Tags))
            {
                FileLog._.D("Current FileManifest Is Null");
                return false;
            }

            System.IO.File.Move(FileSetting.CacheDir + FileSetting.ManifestUpgradeName, FileSetting.CacheDir + FileSetting.ManifestName);

            _Manifest_Local = new_manifest;
            _Manifest_Current = new_manifest;
            return true;
        }

        public byte[] ReadAllBytes(string name)
        {
            string full_path = GetFilePath(name);
            if (full_path == null)
                return null;

            if (full_path.StartsWith(FileSetting.StreamingAssetsDir))
            {
                full_path = full_path.Substring(FileSetting.StreamingAssetsDir.Length);
                FileLog._.D("从StreamingAssets 里面读取文件 {0}->{1}", name, full_path);
                return SAFileSystem.ReadAllBytes(full_path);
            }
            if (System.IO.File.Exists(full_path))
            {
                return System.IO.File.ReadAllBytes(full_path);
            }

            FileLog._.D("文件不存在 {0}->{1}", name, full_path);
            return null;
        }

        public void SetBaseTags(List<string> tags)
        {
            _Tags.Clear();
            foreach (var p in tags)
            {
                _Tags.Add(p);
            }
        }

        public string GetFilePath(string name)
        {
            if (_Manifest_Current == null)
            {
                FileLog._.D("Current FileManifest Is Null");
                return null;
            }

            var item = _Manifest_Current.FindFile(name);
            if (item == null)
            {
                FileLog._.D("找不到文件 {0}", name);
                return null;
            }

            return _FileCollection.GetFullPath(item.FullName);
        }

        public EFileStatus GetFileStatus(string name)
        {
            if (_Manifest_Current == null)
                return EFileStatus.NotExist;

            var item = _Manifest_Current.FindFile(name);
            if (item == null)
                return EFileStatus.NotExist;
            if (_FileCollection.IsExist(item.FullName))
                return EFileStatus.Exist;
            return EFileStatus.NotDownloaded;
        }

        private FileManifest _LoadUpgradeManifest()
        {
            string file_path = FileSetting.CacheDir + FileSetting.ManifestUpgradeName;
            if (!System.IO.File.Exists(file_path))
            {
                FileLog._.D("新的FileManifest不存在 {0}", file_path);
                return null;
            }
            string content = System.IO.File.ReadAllText(file_path);
            FileManifest ret = UnityEngine.JsonUtility.FromJson<FileManifest>(content);
            FileLog._.Assert(ret != null, "新的FileManifest加载失败 {0}", file_path);
            return ret;
        }

        private FileManifest _LoadLocalManifest()
        {
            string file_path = FileSetting.CacheDir + FileSetting.ManifestName;
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
            string file_path = FileSetting.StreamingAssetsDir + FileSetting.ManifestName;
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

        /// <summary>
        /// 检查 manifest 里面有 含有 tag FileSetting.TagBase  的文件都存在
        /// </summary>        
        private static bool _CheckFileExists(FileCollection file_collection, FileManifest manifest, HashSet<string> tags)
        {
            if (file_collection == null || manifest == null)
                return false;

            List<FileManifest.FileItem> files = manifest.GetFilesWithTags(tags);
            bool ret = true;

            foreach (var p in files)
            {
                if (!file_collection.IsExist(p.FullName))
                {
                    FileLog._.D("文件 {0} 不存在", p.FullName);
                    ret = true;
#if UNITY_EDITOR
#else
                    break;
#endif
                }
            }

            return ret;
        }
    }
}