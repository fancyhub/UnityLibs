/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.SampleExternalLoader
{
    public sealed class ResExternalLoader_Bundle : CPtrBase, IResMgr.IExternalLoader
    {
        private CPtr<IBundleMgr> _BundleMgr;
        private Func<string, string> _AtlasTag;

        private sealed class AssetRef : CPoolItemBase, IResMgr.IExternalAssetRef
        {
            public UnityEngine.Object _Asset;
            public AssetBundleRequest _ResRequest;
            public SPtr<IBundle> _Bundle;

            public bool IsDone
            {
                get
                {
                    if (Asset != null)
                        return true;
                    if (_ResRequest != null)
                        return _ResRequest.isDone;
                    return true;
                }
            }

            public static AssetRef Create(ICSPtr<IBundle> bundle, UnityEngine.Object asset)
            {
                if (bundle == null || asset == null)
                    return null;

                AssetRef ret = GPool.New<AssetRef>();
                ret._Asset = asset;
                ret._Bundle = new SPtr<IBundle>(bundle);
                return ret;
            }

            public static AssetRef Create(ICSPtr<IBundle> bundle, AssetBundleRequest req)
            {
                if (bundle == null || req == null)
                    return null;

                AssetRef ret = GPool.New<AssetRef>();
                ret._ResRequest = req;
                ret._Bundle = new SPtr<IBundle>(bundle);
                return ret;
            }

            public string BundleName
            {
                get
                {
                    IBundle bundle = _Bundle.Val;
                    return bundle == null ? null : bundle.Name;
                }
            }
            public UnityEngine.Object Asset
            {
                get
                {
                    if (_Asset != null)
                        return _Asset;
                    if (_ResRequest != null && _ResRequest.isDone)
                    {
                        _Asset = _ResRequest.asset;
                        _ResRequest = null;
                    }
                    return _Asset;
                }
            }

            protected override void OnPoolRelease()
            {
                _ResRequest = null;
                _Asset = null;
                _Bundle.Destroy();
                _Bundle = default;
            }
        }


        public ResExternalLoader_Bundle(IBundleMgr bundle_mgr, Func<string, string> atlas_tag)
        {
            _BundleMgr = new CPtr<IBundleMgr>(bundle_mgr);
            _AtlasTag = atlas_tag;
        }

        public string AtlasTag2Path(string atlasName)
        {
            if (_AtlasTag != null)
                return _AtlasTag(atlasName);
            return string.Empty;
        }

        public EAssetStatus GetAssetStatus(string path)
        {
            IBundleMgr bundleMgr = _BundleMgr.Val;
            if (bundleMgr == null)
                return EAssetStatus.NotExist;


            var bundle_info = bundleMgr.GetBundleInfoByAsset(path);
            if (bundle_info.Name == null || bundle_info.Status == EBundleFileStatus.None)
                return EAssetStatus.NotExist;
            else if (bundle_info.Status == EBundleFileStatus.Ready)
                return EAssetStatus.Exist;
            else
                return EAssetStatus.NotDownloaded;
        }

        public IResMgr.IExternalAssetRef LoadAsset(string path, Type unityAssetType)
        {
            IBundleMgr bundleMgr = _BundleMgr.Val;
            if (bundleMgr == null)
                return null;


            ICSPtr<IBundle> bundle = bundleMgr.LoadBundleByAsset(path);
            if (bundle == null)
                return null;

            UnityEngine.Object asset = bundle.Val.LoadAsset(path, unityAssetType);
            if (asset == null)
            {
                bundle.Destroy();
                return null;
            }
            return AssetRef.Create(bundle, asset);
        }

        public IResMgr.IExternalAssetRef LoadAssetAsync(string path, Type unityAssetType)
        {
            IBundleMgr bundleMgr = _BundleMgr.Val;
            if (bundleMgr == null)
                return null;

            ICSPtr<IBundle> bundle = bundleMgr.LoadBundleByAsset(path);
            if (bundle == null)
                return null;

            UnityEngine.AssetBundleRequest req = bundle.Val.LoadAssetAsync(path, unityAssetType);
            if (req == null)
            {
                bundle.Destroy();
                return null;
            }
            return AssetRef.Create(bundle, req);
        }

        protected override void OnRelease()
        {

        }
    }
}
