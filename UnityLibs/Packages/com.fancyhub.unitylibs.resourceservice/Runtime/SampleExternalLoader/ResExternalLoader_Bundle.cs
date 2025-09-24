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
    public sealed class ResExternalLoader_Bundle : CPtrBase, IResMgr.IExternalAssetLoader
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

        public IResMgr.IExternalAssetRef Load(string path, Type unityAssetType)
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
            return  AssetRef.Create(bundle, asset);
        }

        public IResMgr.IExternalAssetRef LoadAsync(string path, Type unityAssetType)
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
            return  AssetRef.Create(bundle, req);
        }

        protected override void OnRelease()
        {

        }
    }
    /*
    public class ResExternalLoader_Bundle : CPtrBase, IResMgr.IExternalLoader
    {
        private CPtr<IBundleMgr> _BundleMgr;
        private Func<string, string> _AtlasTag;
        private sealed class ResRefDB
        {
            public Dictionary<int, int> _Data = new Dictionary<int, int>();
            public void IncRef(UnityEngine.Object obj)
            {
                if (obj == null)
                    return;
                int id = obj.GetInstanceID();
                if (_Data.TryGetValue(id, out var count))
                {
                    _Data[id] = count + 1;
                }
                else
                    _Data.Add(id, 1);
            }

            public int DecRef(UnityEngine.Object obj)
            {
                if (obj == null)
                    return 0;
                int id = obj.GetInstanceID();
                if (!_Data.TryGetValue(id, out var count))
                    return 0;

                count--;
                if (count <= 0)
                    _Data.Remove(id);
                return count;
            }
        }

        private sealed class AssetRef : CPoolItemBase, IResMgr.IExternalRef
        {
            public UnityEngine.Object _Asset;
            public AssetBundleRequest _ResRequest;
            public ResRefDB _ResRefDB;
            public SPtr<IBundle> _Bundle;

            public bool IsDone
            {
                get
                {
                    if (_ResRequest == null)
                        return true;
                    if (!_ResRequest.isDone)
                        return false;
                    _Asset = _ResRequest.asset;
                    _ResRefDB.IncRef(_Asset);
                    _ResRequest = null;
                    return true;
                }
            }

            public static AssetRef Create(ResRefDB res_ref_db, ICSPtr<IBundle> bundle, UnityEngine.Object asset)
            {
                if (res_ref_db == null || bundle == null || asset == null)
                    return null;

                AssetRef ret = GPool.New<AssetRef>();
                ret._ResRefDB = res_ref_db;
                ret._Asset = asset;
                ret._Bundle = new SPtr<IBundle>(bundle);

                res_ref_db.IncRef(asset);
                bundle.IncRef();
                return ret;
            }

            public static AssetRef Create(ResRefDB res_ref_db, IBundle bundle, AssetBundleRequest req)
            {
                if (bundle == null || res_ref_db == null || req == null)
                    return null;

                AssetRef ret = GPool.New<AssetRef>();
                ret._ResRefDB = res_ref_db;
                ret._ResRequest = req;
                ret._Bundle = new SPtr<IBundle>(bundle);
                bundle.IncRef();
                return ret;
            }

            public UnityEngine.Object Asset => _Asset;

            protected override void OnPoolRelease()
            {
                _ResRequest = null;
                if (_Asset == null)
                {
                    _Bundle.Val?.DecRef();
                    _Bundle = null;
                    _ResRefDB = null;
                    return;
                }

                var asset = _Asset;
                var resRefDB = _ResRefDB;
                var bundle = _Bundle;
                _ResRefDB = null;
                _Asset = null;
                _Bundle = null;

                if (resRefDB.DecRef(asset) <= 0)
                {
                    //UnloadAsset may only be used on individual assets and can not be used on GameObject's / Components / AssetBundles or GameManagers
                    if (!(asset is GameObject))
                    {
                        Resources.UnloadAsset(asset);
                    }
                }

                bundle.Val?.DecRef();
            }
        }

        private ResRefDB _ResRefDB = new ResRefDB();

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

        public IResMgr.IExternalRef Load(string path, Type unityAssetType)
        {
            IBundleMgr bundleMgr = _BundleMgr.Val;
            if (bundleMgr == null)
                return null;


            IBundle bundle = bundleMgr.LoadBundleByAsset(path);
            if (bundle == null)
                return null;

            UnityEngine.Object asset = bundle.LoadAsset(path, unityAssetType);

            AssetRef ret = AssetRef.Create(_ResRefDB, bundle, asset);

            bundle.DecRef();
            return ret;
        }

        public IResMgr.IExternalRef LoadAsync(string path, Type unityAssetType)
        {
            IBundleMgr bundleMgr = _BundleMgr.Val;
            if (bundleMgr == null)
                return null;

            IBundle bundle = bundleMgr.LoadBundleByAsset(path);
            if (bundle == null)
                return null;

            UnityEngine.AssetBundleRequest req = bundle.LoadAssetAsync(path, unityAssetType);
            AssetRef ret = AssetRef.Create(_ResRefDB, bundle, req);

            bundle.DecRef();
            return ret;
        }

        protected override void OnRelease()
        {

        }
    }
//*/
}
