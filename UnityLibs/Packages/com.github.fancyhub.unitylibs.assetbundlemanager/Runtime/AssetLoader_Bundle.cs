/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public class AssetLoader_Bundle : CPtrBase, IAssetLoader
    {
        public CPtr<IBundleMgr> _BundleMgr;
        public sealed class ResRefDB
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

        public sealed class AssetRef : CPoolItemBase, IAssetRef
        {
            public UnityEngine.Object _Asset;
            public AssetBundleRequest _ResRequest;
            public ResRefDB _ResRefDB;
            public IBundle _Bundle;

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

            public static AssetRef Create(ResRefDB res_ref_db, IBundle bundle, UnityEngine.Object asset)
            {
                if (res_ref_db == null || bundle == null || asset == null)
                    return null;

                AssetRef ret = GPool.New<AssetRef>();
                ret._ResRefDB = res_ref_db;
                ret._Asset = asset;
                ret._Bundle = bundle;

                res_ref_db.IncRef(asset);
                bundle.IncRefCount();
                return ret;
            }

            public static AssetRef Create(ResRefDB res_ref_db, IBundle bundle, AssetBundleRequest req)
            {
                if (bundle == null || res_ref_db == null || req == null)
                    return null;

                AssetRef ret = GPool.New<AssetRef>();
                ret._ResRefDB = res_ref_db;
                ret._ResRequest = req;
                ret._Bundle = bundle;
                bundle.IncRefCount();
                return ret;
            }

            public UnityEngine.Object Asset => _Asset;

            protected override void OnPoolRelease()
            {
                _ResRequest = null;
                if (_Asset == null)
                {
                    _Bundle?.DecRefCount();
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

                bundle?.DecRefCount();
            }
        }

        public ResRefDB _ResRefDB = new ResRefDB();

        public AssetLoader_Bundle(IBundleMgr bundle_mgr)
        {
            _BundleMgr = new CPtr<IBundleMgr>(bundle_mgr);
        }

        public string AtlasTag2Path(string atlasName)
        {
            return string.Empty;
        }

        public EAssetStatus GetAssetStatus(string path)
        {
            IBundleMgr bundleMgr = _BundleMgr.Val;
            if (bundleMgr == null)
                return EAssetStatus.NotExist;

            IBundle bundle = bundleMgr.GetBundleByAsset(path);
            if (bundle == null)
                return EAssetStatus.NotExist;
            if (bundle.IsDownloaded())
                return EAssetStatus.Exist;
            else
                return EAssetStatus.NotDownloaded;
        }

        public IAssetRef Load(string path, bool sprite)
        {
            IBundleMgr bundleMgr = _BundleMgr.Val;
            if (bundleMgr == null)
                return null;


            IBundle bundle = bundleMgr.LoadBundleByAsset(path);
            if (bundle == null)
                return null;

            UnityEngine.Object asset = null;
            if (sprite)
                asset = bundle.LoadAsset<Sprite>(path);
            else
                asset = bundle.LoadAsset<UnityEngine.Object>(path);

            AssetRef ret = AssetRef.Create(_ResRefDB, bundle, asset);

            bundle.DecRefCount();
            return ret;
        }

        public IAssetRef LoadAsync(string path, bool sprite)
        {
            IBundleMgr bundleMgr = _BundleMgr.Val;
            if (bundleMgr == null)
                return null;

            IBundle bundle = bundleMgr.LoadBundleByAsset(path);
            if (bundle == null)
                return null;

            UnityEngine.AssetBundleRequest req = null;
            if (sprite)
                req = bundle.LoadAssetAsync<Sprite>(path);
            else
                req = bundle.LoadAssetAsync<UnityEngine.Object>(path);
            AssetRef ret = AssetRef.Create(_ResRefDB, bundle, req);

            bundle.DecRefCount();
            return ret;
        }

        protected override void OnRelease()
        {

        }
    }
}
