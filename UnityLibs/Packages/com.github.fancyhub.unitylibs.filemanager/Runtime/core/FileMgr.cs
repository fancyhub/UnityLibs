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
    public enum EFileStatus
    {
        NotExist,
        Exist,
        NotDownloaded,
    }

    public interface IFileMgr : ICPtr
    {
        public VersionInfo GetVersionInfo();
        public bool UpgradeManifest();
        public void SetBaseTags(List<string> tags);

        public string GetFilePath(string name);
        public EFileStatus GetFileStatus(string name);

        public byte[] ReadAllBytes(string name);
    }

    public static class FileMgr
    {
        private static CPtr<IFileMgr> _;

        public static IFileMgr Inst { get { return _.Val; } }

        public static void Init(FileMgrConfig config)
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

            FileMgrImplement file_mgr = new FileMgrImplement();
            _ = new CPtr<IFileMgr>(file_mgr);
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
        public static bool UpgradeManifest()
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return false;
            }
            return mgr.UpgradeManifest();
        }
        public static void SetBaseTags(List<string> tags)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return;
            }
            mgr.SetBaseTags(tags);
        }

        public static string GetFilePath(string name)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return null;
            }
            return mgr.GetFilePath(name);
        }

        public static EFileStatus GetFileStatus(string name)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                FileLog._.E("FileMgr Is Null");
                return EFileStatus.NotExist;
            }
            return mgr.GetFileStatus(name);
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

        public static void Destroy()
        {
            _.Destroy();
        }
    }
}