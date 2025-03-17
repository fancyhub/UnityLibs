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
        private CPtr<IBundleMgr.IExternalLoader> _ExternalLoader;
        private List<Bundle> _BundleList;
        private MyDict<string, Bundle> _AssetDict;

        public BundleManifest _Config;
        public void Init(IBundleMgr.IExternalLoader external_loader, BundleManifest config)
        {
            _ExternalLoader = new CPtr<IBundleMgr.IExternalLoader>(external_loader);
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


        public IBundle FindBundleByAsset(string asset)
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

        public IBundleMgr.EBundleFileStatus GetBundleStatus(IBundle bundle)
        {
            if (bundle == null)
            {
                BundleLog.Assert(false, "param bundle is null");
                return IBundleMgr.EBundleFileStatus.None;
            }

            var loader = _ExternalLoader.Val;
            if (loader == null)
            {
                BundleLog.Assert(false, "external loader is null");
                return IBundleMgr.EBundleFileStatus.None;
            }

            return loader.GetBundleFileStatus(bundle.Name);
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

            b.IncRefCount();
            return b;
        }

        protected override void OnRelease()
        {
            foreach (var p in _BundleList)
            {
                p.Dispose();
            }
            _BundleList.Clear();
            _AssetDict.Clear();
        }
    }
}
