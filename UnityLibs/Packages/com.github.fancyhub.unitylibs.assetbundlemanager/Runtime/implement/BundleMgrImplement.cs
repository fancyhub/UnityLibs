/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.ABManagement
{
    internal class BundleMgrImplement : CPtrBase, IBundleMgr
    {
        private static Dictionary<string, Bundle> _S_TempDict = new Dictionary<string, Bundle>();
        private static List<Bundle> _S_TempList = new List<Bundle>();

        private CPtr<IBundleMgr.IExternalLoader> _ExternalLoader;
        private List<Bundle> _BundleList;
        private MyDict<string, Bundle> _AssetDict;


        public BundleManifest _Config;
        public void Init(IBundleMgr.IExternalLoader external_loader, BundleManifest config)
        {
            _ExternalLoader = new CPtr<IBundleMgr.IExternalLoader>(external_loader);
            _CreateBundles(config);
        }

        public void GetBundleInfoByAssets(List<string> asset_list, List<BundleInfo> out_bundle_stat_list)
        {
            out_bundle_stat_list.Clear();

            _S_TempDict.Clear();
            foreach (var p in asset_list)
            {
                Bundle bundle = _FindBundleByAsset(p);
                if (bundle == null)
                    continue;
                if (_S_TempDict.ContainsKey(bundle.Name))
                    continue;
                _S_TempDict.Add(bundle.Name, bundle);

                if (bundle._AllDeps != null && bundle._AllDeps.Length > 0)
                {
                    foreach (var p2 in bundle._AllDeps)
                    {
                        _S_TempDict[p2.Name] = bundle;
                    }
                }
            }

            foreach (var p in _S_TempDict)
            {
                var status = _GetBundleStatus(p.Key);
                out_bundle_stat_list.Add(new(p.Key, status));
            }
        }

        public BundleInfo GetBundleInfoByAsset(string asset)
        {
            Bundle b = _FindBundleByAsset(asset);
            if (b == null)
                return new BundleInfo(null, EBundleFileStatus.None);
            var status = _GetBundleStatus(b.Name);
            return new BundleInfo(b.Name, status);
        }


        public Bundle _FindBundleByAsset(string asset)
        {
            if (string.IsNullOrEmpty(asset))
            {
                BundleLog.Assert(false, "param asset is null");
                return null;
            }

            _AssetDict.TryGetValue(asset, out Bundle b);
            if (b == null)
            {
                BundleLog.E("找不到 {0} 对应的bundle", asset);
            }
            return b;
        }

        public EBundleFileStatus _GetBundleStatus(string bundle_name)
        {
            var loader = _ExternalLoader.Val;
            if (loader == null)
            {
                BundleLog.Assert(false, "external loader is null");
                return EBundleFileStatus.None;
            }
            return loader.GetBundleFileStatus(bundle_name);
        }

        public void GetAllBundles(List<IBundle> bundles)
        {
            bundles.Clear();
            bundles.AddRange(_BundleList);
        }

        public IBundle LoadBundleByAsset(string asset)
        {
            if (string.IsNullOrEmpty(asset))
                return null;

            _AssetDict.TryGetValue(asset, out Bundle b);
            if (b == null)
            {
                BundleLog.E("找不到 {0} 对应的bundle", asset);
                return null;
            }

            if (!b.Load())
            {
                BundleLog.E("加载失败 Asset:{0}, 对应的Bundle: {1}", asset, b.Name);
                return null;
            }

            b.IncRef();
            return b;
        }

        protected override void OnRelease()
        {
            foreach (var p in _BundleList)
            {
                p.Destroy();
            }
            _BundleList.Clear();
            _AssetDict.Clear();
        }

        public void Upgrade()
        {
            if (_ExternalLoader.Val == null)
                return;
            var new_manifest = _ExternalLoader.Val.LoadManifest();
            if (new_manifest == null)
                return;

            BundleDef.UnloadAllLoadedObjectsCurrent = false;
            foreach (var p in _BundleList)
                p.Destroy();
            BundleDef.UnloadAllLoadedObjectsCurrent = BundleDef.UnloadAllLoadedObjectsDefault;

            _CreateBundles(new_manifest);
        }

        private void _CreateBundles(BundleManifest config)
        {
            _Config = config;
            _BundleList = new List<Bundle>(config.BundleList.Length);
            _AssetDict = new MyDict<string, Bundle>();

            foreach (var p in config.BundleList)
            {
                Bundle b = new Bundle();
                b._Config = p;
                b._ExternalLoader = _ExternalLoader;
                _BundleList.Add(b);

                foreach (var a in p.GetAssets())
                {
                    _AssetDict.Add(a, b);
                }
            }

            List<int> tempList = new List<int>();
            for (int i = 0; i < config.BundleList.Length; i++)
            {
                config.GetAllDeps(i, tempList);

                _BundleList[i]._AllDeps = new Bundle[tempList.Count];
                for (int j = 0; j < tempList.Count; j++)
                {
                    _BundleList[i]._AllDeps[j] = _BundleList[tempList[j]];
                }
            }
        }
    }
}
