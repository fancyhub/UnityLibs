/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        
        public IBundleMgr.IExternalRef LoadBundleFile(string name)
        {
            return BundleItem.LoadFromFile(System.IO.Path.Combine(_Dir, name));
        }

        public BundleManifest LoadManifest()
        {
            string path = System.IO.Path.Combine(_Dir, _BundleManifestName);
            return BundleManifest.LoadFromFile(path);
        }

        protected override void OnRelease()
        {
        }

        public sealed class BundleItem : IBundleMgr.IExternalRef
        {
            private AssetBundle _Bundle;

            public static BundleItem LoadFromFile(string file_path)
            {
                UnityEngine.AssetBundle ab = UnityEngine.AssetBundle.LoadFromFile(file_path);
                if (ab == null)
                    return null;
                BundleItem ret = new BundleItem();
                ret._Bundle = ab;
                return ret;
            }

            public AssetBundle UnityBundle => _Bundle;
          

            public UnityEngine.Object LoadAsset(string name, Type unityAssetType)
            {
                return _Bundle.LoadAsset(name, unityAssetType);
            }        

            public AssetBundleRequest LoadAssetAsync(string name, Type unityAssetType)
            {
                return _Bundle.LoadAssetAsync(name, unityAssetType);
            }

            public void UnloadBundle(bool unloadAllLoadedObjects)
            {
                if (_Bundle != null)
                {
                    var t = _Bundle;
                    _Bundle = null;
                    t.Unload(unloadAllLoadedObjects);
                }
            }
        }
    }
}
