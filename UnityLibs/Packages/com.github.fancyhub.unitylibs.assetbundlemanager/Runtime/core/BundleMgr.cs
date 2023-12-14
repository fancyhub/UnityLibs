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
        public string GetBundleFullPath(string name);
        public EBundleFileStatus GetBundleFileStatus(string name);
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

        public static bool InitMgr(IBundleLoader bundle_loader, BundleMgrManifest config)
        {
            if (_.Val != null)
                return false;

            AB.BundleMgrImplement mgr = new AB.BundleMgrImplement();
            mgr.Init(bundle_loader, config);
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
