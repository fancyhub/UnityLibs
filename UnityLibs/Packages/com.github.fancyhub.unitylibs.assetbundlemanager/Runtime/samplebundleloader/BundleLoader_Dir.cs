/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace FH.ABManagement.SampleBundleLoader
{
    public class BundleLoader_Dir : CPtrBase, IBundleLoader
    {
        public string _Dir;
        public string _BundleManifestName;
        public BundleLoader_Dir(string dir, string bundleManifestName)
        {
            _Dir = dir;
            _BundleManifestName = bundleManifestName;
        }

        public EBundleFileStatus GetBundleFileStatus(string name)
        {
            string path = System.IO.Path.Combine(_Dir, name);
            if (System.IO.File.Exists(path))
                return EBundleFileStatus.Exist;
            else
                return EBundleFileStatus.NoExist;
        }

        public string GetBundleFilePath(string name)
        {
            return System.IO.Path.Combine(_Dir, name);
        }

        public Stream LoadBundleFile(string name)
        {
            return null;
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