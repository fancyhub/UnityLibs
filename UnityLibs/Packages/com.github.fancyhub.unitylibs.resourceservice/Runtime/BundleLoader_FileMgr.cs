/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    public class BundleLoader_FileMgr : CPtrBase, IBundleLoader
    {
        public string _BundleManifestName;
        public CPtr<IFileMgr> _FileMgr;

        public BundleLoader_FileMgr(IFileMgr fileMgr, string manifest_name)
        {
            _FileMgr = new CPtr<IFileMgr>(fileMgr);
            _BundleManifestName = manifest_name;
        }

        public EBundleFileStatus GetBundleFileStatus(string name)
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return EBundleFileStatus.NoExist;
            switch (mgr.GetFileStatus(name))
            {
                case EFileStatus.NotExist:
                    return EBundleFileStatus.NoExist;
                case EFileStatus.Exist:
                    return EBundleFileStatus.Exist;
                case EFileStatus.NotDownloaded:
                    return EBundleFileStatus.NeedDownload;
                default:
                    return EBundleFileStatus.NoExist;
            }
        }

        public string GetBundleFilePath(string name)
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return null;
            return mgr.GetFilePath(name);
        }

        public Stream LoadBundleFile(string name)
        {
            return null;
        }

        public BundleManifest LoadManifest()
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return null;

            byte[] bytes=mgr.ReadAllBytes(_BundleManifestName);
            if (bytes == null)
                return null;
            string content = System.Text.Encoding.UTF8.GetString(bytes);
            return UnityEngine.JsonUtility.FromJson<BundleManifest>(content);
        }

        protected override void OnRelease()
        {
        }

    }
}
