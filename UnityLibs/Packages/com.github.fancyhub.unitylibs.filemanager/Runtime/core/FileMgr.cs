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
        Remote, //在远程, 还没有下载
    }

    public partial interface IFileMgr : ICPtr
    {
        public VersionInfo GetVersionInfo();
        public FileManifest GetCurrentManifest();

        public bool Upgrade(FileManifest new_manifest, List<FileManifest.FileItem> out_need_download_list = null);

        public EFileStatus FindFile(string name, out string full_path);

        public bool IsAllReady(FileManifest manifest, HashSet<string> tags = null, List<FileManifest.FileItem> out_need_download_list = null);

        public ExtractStreamingAssetsOperation GetExtractOperation();

        public void RefreshFileList();

        public System.IO.Stream OpenRead(string name);
        public byte[] ReadAllBytes(string name);
    }

    public static class FileMgr
    {
        private static CPtr<IFileMgr> _;
        private static HashSet<string> _S_TempTags = new HashSet<string>();

        public static IFileMgr Inst { get { return _.Val; } }

        public static void Init(IFileMgr.Config config)
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
            FileLog._ = TagLogger.Create(FileLog._.Tag, config.LogLvl);

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

        public static bool IsAllReady(FileManifest manifest, List<string> tags = null, List<FileManifest.FileItem> out_need_download_list = null)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return false;
            }

            if (tags == null)
            {
                return mgr.IsAllReady(manifest, null, out_need_download_list);
            }

            _S_TempTags.Clear();
            foreach (var p in tags)
            {
                if (string.IsNullOrEmpty(p))
                    continue;
                _S_TempTags.Add(p);
            }
            return mgr.IsAllReady(manifest, null, out_need_download_list);
        }

        public static EFileStatus FindFile(string name, out string full_path)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                full_path = null;
                return EFileStatus.None;
            }

            return mgr.FindFile(name, out full_path);
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

        public static void Destroy()
        {
            _.Destroy();
        }
    }
}
