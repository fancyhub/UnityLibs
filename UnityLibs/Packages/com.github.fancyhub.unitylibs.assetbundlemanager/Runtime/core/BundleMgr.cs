/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.ABManagement;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace FH
{
    public enum EBundleFileStatus
    {
        Exist,
        NoExist,
        NeedDownload,
        Downloading,
    }

    public interface IBundleLoader : ICPtr
    {
        /// <summary>
        /// 如果返回null, 使用 AssetBundle.LoadFromFile
        /// 如果返回正确的值, 使用 AssetBundle.LoadFromStream
        /// </summary>
        public Stream LoadBundleFile(string name);
        public string GetBundleFilePath(string name);
        public EBundleFileStatus GetBundleFileStatus(string name);

        public BundleManifest LoadManifest();
    }

    public interface IBundle
    {
        public string Name { get; }

        public void GetAllDeps(List<IBundle> deps);

        public bool IsDownloaded();

        public int IncRefCount();
        public int DecRefCount();
        public int RefCount { get; }

        public T LoadAsset<T>(string path) where T : UnityEngine.Object;

        public AssetBundleRequest LoadAssetAsync<T>(string path) where T : UnityEngine.Object;
    }

    public interface IBundleMgr : ICPtr
    {
        public IBundle LoadBundleByAsset(string asset);

        public IBundle GetBundleByAsset(string asset);

        public void GetAllBundles(List<IBundle> bundles);
    }


    public static class BundleMgr
    {
        private static CPtr<IBundleMgr> _;

        public static IBundleMgr Inst { get { return _.Val; } }

        public static bool InitMgr(BundleMgrConfig config, IBundleLoader bundle_loader)
        {
            if (config == null)
            {
                BundleLog._.E("BundleMgrConfig is Null");
                return false;
            }

            if (_.Val != null)
            {
                BundleLog._.E("BundleMgr 已经创建了");
                return false;
            }

            if (bundle_loader == null)
            {
                BundleLog._.E("BundlerLoader is Null");
                return false;
            }

            BundleLog._ = TagLogger.Create(BundleLog._.Tag, config.LogLvl);

            var manifest = bundle_loader.LoadManifest();
            if (manifest == null)
            {
                BundleLog._.E("bundle manifest is Null");
                return false;
            }


            ABManagement.BundleMgrImplement mgr = new ABManagement.BundleMgrImplement();
            mgr.Init(bundle_loader, manifest);
            _ = mgr;
            return true;
        }

        public static void Destroy()
        {
            _.Destroy();
        }

        public static IBundle LoadBundleByAsset(string asset)
        {
            return _.Val.LoadBundleByAsset(asset);
        }
    }
}
