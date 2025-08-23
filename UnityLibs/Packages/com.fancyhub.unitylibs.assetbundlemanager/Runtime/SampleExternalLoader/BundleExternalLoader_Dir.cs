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
    public class BundleExternalLoader_Dir : CPtrBase, IBundleMgr.IExternalLoader
    {
        public string _Dir;
        public string _BundleManifestName;
        public BundleExternalLoader_Dir(string dir, string bundleManifestName)
        {
            _Dir = dir;
            _BundleManifestName = bundleManifestName;
        }

        public EBundleFileStatus GetBundleFileStatus(string name)
        {
            string path = System.IO.Path.Combine(_Dir, name);
            if (System.IO.File.Exists(path))
                return EBundleFileStatus.Ready;
            else
                return EBundleFileStatus.None;
        }
        
        public IBundleMgr.ExternalBundle LoadBundleFile(string name)
        {
            return IBundleMgr.ExternalBundle.LoadFromFile(System.IO.Path.Combine(_Dir, name));
        }

        public BundleManifest LoadManifest()
        {
            string path = System.IO.Path.Combine(_Dir, _BundleManifestName);
            return BundleManifest.LoadFromFile(path);
        }

        protected override void OnRelease()
        {
        }
    }
}
