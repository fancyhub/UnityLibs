/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace FH.SampleExternalLoader
{
    public class BundleExternalLoader_FileMgr : CPtrBase, IBundleMgr.IExternalLoader
    {
        public string _BundleManifestName;
        public CPtr<IFileMgr> _FileMgr;

        public BundleExternalLoader_FileMgr(IFileMgr fileMgr, string manifest_name)
        {
            _FileMgr = new CPtr<IFileMgr>(fileMgr);
            _BundleManifestName = manifest_name;
        }

        public IBundleMgr.EBundleFileStatus GetBundleFileStatus(string name)
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return IBundleMgr.EBundleFileStatus.None;
            switch (mgr.FindFile(name, out var _, out var _))
            {
                case EFileStatus.None:
                    return IBundleMgr.EBundleFileStatus.None;
                case EFileStatus.Ready:
                    return IBundleMgr.EBundleFileStatus.Ready;
                case EFileStatus.NeedDownload:
                    return IBundleMgr.EBundleFileStatus.Remote;
                default:
                    return IBundleMgr.EBundleFileStatus.None;
            }
        }

        public string GetBundleFilePath(string name)
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return null;
            mgr.FindFile(name, out var ret, out var _);
            return ret;
        }

        public IBundleMgr.ExternalBundle LoadBundleFile(string name)
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return null;
            mgr.FindFile(name, out var ret, out var _);
            if (ret == null)
                return null;
            var ab = UnityEngine.AssetBundle.LoadFromFile(ret);
            return IBundleMgr.ExternalBundle.Create(ab);
        }

        public BundleManifest LoadManifest()
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return null;

            byte[] bytes = mgr.ReadAllBytes(_BundleManifestName);
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
