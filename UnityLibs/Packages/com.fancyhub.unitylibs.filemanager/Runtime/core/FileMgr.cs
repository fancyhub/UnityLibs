/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/
using FH.FileManagement;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public abstract class ExtractStreamingAssetsOperation : CustomYieldInstruction
    {
        public abstract bool IsDone { get; }
        public abstract float Progress { get; }
    }

    public enum EFileStatus
    {
        None, //不存在
        Ready, //可以直接读取
        NeedDownload, //在远程, 还没有下载
    }

    public enum EFileLocation
    {
        None, //
        StreamingAssets,
        Persistent,
        Remote,
    }

    public partial interface IFileMgr : ICPtr
    {
        public VersionInfo GetVersionInfo();
        public FileManifest GetCurrentManifest();

        public bool Upgrade(FileManifest new_manifest, List<FileManifest.FileItem> out_need_download_list = null);

        public EFileStatus FindFile(string name, out string full_path, out EFileLocation file_location);

        public EFileStatus FindFile(FileManifest.FileItem file, out string full_path, out EFileLocation file_location);

        public bool IsAllTagsReady(FileManifest manifest, HashSet<string> tags = null, List<FileManifest.FileItem> out_need_download_list = null);

        public bool IsAllFilesReady(FileManifest manifest, HashSet<string> file_names, List<FileManifest.FileItem> out_need_download_list = null);

        public ExtractStreamingAssetsOperation GetExtractOperation();

        public void OnFileDownload(FileManifest.FileItem item);

        public void RefreshFileList();

        public System.IO.Stream OpenRead(string name);
        public byte[] ReadAllBytes(string name);

        public List<(string file_path, bool can_delete)> GetAllFiles(FileManifest new_manifest = null);
    }

    public static class FileMgr
    {
        public struct FileInfo
        {
            public FileManifest.FileItem Base;
            public string FullPath;
            public EFileLocation Location;
            public EFileStatus Status;
        }

        private static CPtr<IFileMgr> _;
        private static HashSet<string> _S_TempStringHashSet = new HashSet<string>();

        public static IFileMgr Inst { get { return _.Val; } }

        public static void InitMgr(IFileMgr.Config config, bool disable_in_editor)
        {
            if (config == null)
            {
                FileLog._.E("Config Is Null");
                return;
            }
            if (_.Val != null)
            {
                FileLog._.E("FileMgr 已经创建了");
                return;
            }
            FileLog._ = TagLog.Create(FileLog._.Tag, config.LogLvl);

            if (disable_in_editor && Application.isEditor)
            {
                _ = new FileMgrImplementEmpty();
                return;
            }

            FileMgrImplement file_mgr = new FileMgrImplement(config);
            file_mgr.Init();
            _ = new CPtr<IFileMgr>(file_mgr);
        }

        public static ExtractStreamingAssetsOperation GetExtractOperation()
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return default;
            }
            return mgr.GetExtractOperation();
        }

        public static void RefreshFileList()
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return;
            }
            mgr.RefreshFileList();
        }

        public static VersionInfo GetVersionInfo()
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return default;
            }
            return mgr.GetVersionInfo();
        }

        public static bool Upgrade(FileManifest new_manifest, List<FileManifest.FileItem> out_need_download_list = null)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return false;
            }
            return mgr.Upgrade(new_manifest, out_need_download_list);
        }

        public static bool IsAllTagsReady(FileManifest manifest, List<string> tags = null, List<FileManifest.FileItem> out_need_download_list = null)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return false;
            }

            if (tags == null)
            {
                return mgr.IsAllTagsReady(manifest, null, out_need_download_list);
            }

            _S_TempStringHashSet.Clear();
            foreach (var p in tags)
            {
                if (string.IsNullOrEmpty(p))
                    continue;
                _S_TempStringHashSet.Add(p);
            }
            return mgr.IsAllTagsReady(manifest, _S_TempStringHashSet, out_need_download_list);
        }

        public static bool IsAllTagsReady(List<string> tags = null, List<FileManifest.FileItem> out_need_download_list = null)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return false;
            }
            if (tags == null)
            {
                return mgr.IsAllTagsReady(mgr.GetCurrentManifest(), null, out_need_download_list);
            }
            _S_TempStringHashSet.Clear();
            foreach (var p in tags)
            {
                if (string.IsNullOrEmpty(p))
                    continue;
                _S_TempStringHashSet.Add(p);
            }
            return mgr.IsAllTagsReady(mgr.GetCurrentManifest(), _S_TempStringHashSet, out_need_download_list);
        }

        public static bool IsAllFilesReady(List<string> files, List<FileManifest.FileItem> out_need_download_list = null)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return false;
            }
            if (files == null)
                return true;

            _S_TempStringHashSet.Clear();
            foreach (var p in files)
            {
                if (string.IsNullOrEmpty(p))
                    continue;
                _S_TempStringHashSet.Add(p);
            }

            return mgr.IsAllFilesReady(mgr.GetCurrentManifest(), _S_TempStringHashSet, out_need_download_list);
        }

        public static EFileStatus FindFile(string name, out string full_path, out EFileLocation file_location)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                full_path = null;
                file_location = EFileLocation.None;
                return EFileStatus.None;
            }

            return mgr.FindFile(name, out full_path, out file_location);
        }

        public static bool GetFilesByTags(List<string> tags, List<FileInfo> out_file_list)
        {
            out_file_list.Clear();
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return false;
            }

            var file_manifest = mgr.GetCurrentManifest();
            if (file_manifest == null)
            {
                FileLog._.E("current manifest is null");
                return false;
            }

            _S_TempStringHashSet.Clear();
            foreach (var p in tags)
            {
                if (string.IsNullOrEmpty(p))
                    continue;
                _S_TempStringHashSet.Add(p);
            }

            var file_list = file_manifest.GetFilesWithTags(_S_TempStringHashSet);
            foreach (var p in file_list)
            {
                FileInfo file_info = new FileInfo();
                file_info.Base = p;
                file_info.Status = mgr.FindFile(p, out file_info.FullPath, out file_info.Location);
                out_file_list.Add(file_info);
            }

            return true;
        }

        public static System.IO.Stream OpenRead(string name)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return null;
            }
            return mgr.OpenRead(name);
        }

        public static byte[] ReadAllBytes(string name)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return null;
            }
            return mgr.ReadAllBytes(name);
        }

        public static void OnFileDownloaded(FileManifest.FileItem item)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return;
            }

            mgr.OnFileDownload(item);
        }

        public static FileManifest GetCurrentManifest()
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return null;
            }
            return mgr.GetCurrentManifest();
        }

        public static List<(string file_path, bool can_delete)> GetAllFiles(FileManifest new_manifest = null)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return null;
            }
            return mgr.GetAllFiles(new_manifest);
        }

        public static void DeleteUnusedFiles(FileManifest new_manifest = null)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return;
            }
            var list = mgr.GetAllFiles(new_manifest);

            foreach (var p in list)
            {
                if (p.can_delete)
                {
                    try
                    {
                        FileLog._.D("Delete file, {0}", p.file_path);
                        System.IO.File.Delete(p.file_path);
                    }
                    catch (System.Exception e)
                    {
                        FileLog._.E("Delete file failed, {0}", p.file_path);
                        FileLog._.E(e);
                    }
                }
            }
            mgr.RefreshFileList();
        }

        public static void Destroy()
        {
            _.Destroy();
        }
    }
}
