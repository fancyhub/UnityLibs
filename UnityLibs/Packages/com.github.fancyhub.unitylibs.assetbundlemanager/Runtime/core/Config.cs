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
    [Serializable]
    public class BundleMgrConfig
    {
        private static HashSet<int> S_TempSet = new HashSet<int>();

        [Serializable]
        public class BundleConfig
        {
            public string Name;
            public int[] Deps;
            public string[] Assets;

            public int[] GetDeps() { return Deps == null ? System.Array.Empty<int>() : Deps; }
            public string[] GetAssets() { return Assets == null ? System.Array.Empty<string>() : Assets; }
        }

        public BundleConfig[] BundleList;

#if UNITY_EDITOR
        public static BundleMgrConfig EdCreateFromManifest(AssetBundleManifest manifest, UnityEditor.AssetBundleBuild[] bundleBuildList)
        {
            var all_bundles = manifest.GetAllAssetBundles();
            List<BundleConfig> bundleList = new List<BundleConfig>(all_bundles.Length);
            Dictionary<string, int> bundleDict = new(all_bundles.Length);

            foreach (var p in all_bundles)
            {
                var bundleConfig = new BundleConfig() { Name = p };
                bundleDict.Add(p, bundleList.Count);
                bundleList.Add(bundleConfig);
            }

            //依赖
            foreach (var p in bundleDict)
            {
                string[] deps = manifest.GetDirectDependencies(p.Key);
                int[] int_deps = new int[deps.Length];
                for (int i = 0; i < deps.Length; i++)
                {
                    int_deps[i] = bundleDict[deps[i]];
                }

                bundleList[p.Value].Deps = int_deps;
            }

            List<string> assetList = new List<string>();
            List<string> sceneList = new List<string>();

            //资源
            foreach (var p in bundleBuildList)
            {
                BundleConfig config = null;
                if (string.IsNullOrEmpty(p.assetBundleVariant))
                    config = bundleList[bundleDict[p.assetBundleName]];
                else
                    config = bundleList[bundleDict[p.assetBundleVariant]];

                if (p.addressableNames != null && p.addressableNames.Length == p.assetNames.Length)
                {
                    for (int i = 0; i < p.assetNames.Length; i++)
                    {
                        assetList.Add(p.addressableNames[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < p.assetNames.Length; i++)
                    {
                        assetList.Add(p.assetNames[i]);
                    }
                }

                config.Assets = assetList.ToArray();
            }

            return new BundleMgrConfig() { BundleList = bundleList.ToArray() };
        }
#endif

        public void GetAllDeps(int index, List<int> out_list)
        {
            out_list.Clear();
            S_TempSet.Clear();

            foreach (var p in BundleList[index].GetDeps())
            {
                if (S_TempSet.Add(p))
                    out_list.Add(p);
            }

            if (out_list.Count == 0)
                return;

            int it_index = 0;
            for (; ; )
            {
                if (it_index >= out_list.Count)
                    return;
                index = out_list[it_index];
                it_index++;

                foreach (var p in BundleList[index].GetDeps())
                {
                    if (S_TempSet.Add(p))
                        out_list.Add(p);
                }
            }
        }
    }
}
