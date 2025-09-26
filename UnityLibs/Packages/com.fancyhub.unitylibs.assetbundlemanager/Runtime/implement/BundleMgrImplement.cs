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

        public BundleManifest _config;

        public CPtr<IBundleMgr.IExternalLoader> _external_loader;
        public List<Bundle> _bundle_list;
        public MyDict<string, Bundle> _asset_2_bundle_map;

        public void Init(IBundleMgr.IExternalLoader external_loader, BundleManifest config)
        {
            _external_loader = new CPtr<IBundleMgr.IExternalLoader>(external_loader);
            _CreateBundles(config);
        }

        public ICSPtr<IBundle> LoadBundleByAsset(string asset)
        {
            Bundle bundle = _FindBundleByAsset(asset);
            if (bundle == null)
                return null;

            if (!bundle.LoadByExternal())
            {
                BundleLog.E("加载失败 Asset:{0}, 对应的Bundle: {1}", asset, bundle.Name);
                return null;
            }

            return bundle.ExtCreateCSPtr<IBundle>();
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

                if (bundle._all_deps != null && bundle._all_deps.Length > 0)
                {
                    foreach (var p2 in bundle._all_deps)
                    {
                        _S_TempDict[p2.Name] = bundle;
                    }
                }
            }

            foreach (var p in _S_TempDict)
            {
                var status = _GetFiletatus(p.Key);
                out_bundle_stat_list.Add(new(p.Key, status));
            }
        }

        public BundleInfo GetBundleInfoByAsset(string asset)
        {
            Bundle b = _FindBundleByAsset(asset);
            if (b == null)
                return new BundleInfo(null, EBundleFileStatus.None);
            var status = _GetFiletatus(b.Name);
            return new BundleInfo(b.Name, status);
        }

        public void Upgrade()
        {
            if (_external_loader.Val == null)
                return;
            var new_manifest = _external_loader.Val.LoadManifest();
            if (new_manifest == null)
                return;

            BundleDef.UnloadAllLoadedObjectsCurrent = false;
            foreach (var p in _bundle_list)
                p.Destroy();
            BundleDef.UnloadAllLoadedObjectsCurrent = BundleDef.UnloadAllLoadedObjectsDefault;

            _CreateBundles(new_manifest);
        }

        public BundleManifest GetBundleManifest()
        {
            return _config;
        }

        public void Snapshot(ref List<BundleSnapshotItem> out_snapshot)
        {
            foreach (var p in _bundle_list)
            {
                out_snapshot.Add(new BundleSnapshotItem()
                {
                    BundleName = p.Name,
                    BundleStatus = p.Status,
                    FileStatus = _GetFiletatus(p.Name),
                });
            }
        }

        protected override void OnRelease()
        {
            foreach (var p in _bundle_list)
            {
                p.Destroy();
            }
            _bundle_list.Clear();
            _asset_2_bundle_map.Clear();
        }

        public Bundle _FindBundleByAsset(string asset)
        {
            if (string.IsNullOrEmpty(asset))
            {
                BundleLog.Assert(false, "param asset is null");
                return null;
            }

            _asset_2_bundle_map.TryGetValue(asset, out Bundle ret);
            if (ret == null)
            {
                BundleLog.E("找不到 {0} 对应的bundle", asset);
            }
            return ret;
        }

        public EBundleFileStatus _GetFiletatus(string bundle_name)
        {
            var loader = _external_loader.Val;
            if (loader == null)
            {
                BundleLog.Assert(false, "external loader is null");
                return EBundleFileStatus.None;
            }
            return loader.GetBundleFileStatus(bundle_name);
        }

        private void _CreateBundles(BundleManifest config)
        {
            _config = config;
            _bundle_list = new List<Bundle>(config.BundleList.Length);
            _asset_2_bundle_map = new MyDict<string, Bundle>();

            foreach (var p in config.BundleList)
            {
                Bundle b = new Bundle();
                b._config = p;
                b._external_loader = _external_loader;
                _bundle_list.Add(b);

                foreach (var a in p.GetAssets())
                {
                    _asset_2_bundle_map.Add(a, b);
                }
            }

            List<int> tempList = new List<int>();
            for (int i = 0; i < config.BundleList.Length; i++)
            {
                config.GetAllDeps(i, tempList);

                _bundle_list[i]._all_deps = new Bundle[tempList.Count];
                for (int j = 0; j < tempList.Count; j++)
                {
                    _bundle_list[i]._all_deps[j] = _bundle_list[tempList[j]];
                }
            }
        }
    }
}
