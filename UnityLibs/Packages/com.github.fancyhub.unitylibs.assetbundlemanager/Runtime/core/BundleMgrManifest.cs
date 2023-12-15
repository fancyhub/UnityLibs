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
    public class BundleMgrManifest
    {
        private static HashSet<int> S_TempSet = new HashSet<int>();


        [Serializable]
        public class BundleManifest
        {
            public string Name;
            public int[] Deps;
            public string[] Assets;

            public int[] GetDeps() { return Deps == null ? System.Array.Empty<int>() : Deps; }
            public string[] GetAssets() { return Assets == null ? System.Array.Empty<string>() : Assets; }
        }

        public BundleManifest[] BundleList;


        public static BundleMgrManifest LoadFromFile(string path)
        {
            if (!System.IO.File.Exists(path))                
            {
                Log.E("文件不存在 " + path);
                return null;
            }

            try
            {
                string content = System.IO.File.ReadAllText(path);
                return UnityEngine.JsonUtility.FromJson<BundleMgrManifest>(content);                
            }
            catch(Exception e)
            {
                Log.E(e);
                return null;
            }            
        }

#if UNITY_EDITOR
        public static BundleMgrManifest EdCreateFromManifest(AssetBundleManifest manifest, UnityEditor.AssetBundleBuild[] bundleBuildList)
        {
            var all_bundles = manifest.GetAllAssetBundles();
            List<BundleManifest> bundleList = new List<BundleManifest>(all_bundles.Length);
            Dictionary<string, int> bundleDict = new(all_bundles.Length);

            foreach (var p in all_bundles)
            {
                var bundleConfig = new BundleManifest() { Name = p };
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

            //资源
            foreach (var p in bundleBuildList)
            {
                assetList.Clear();
                BundleManifest config = null;
                if (string.IsNullOrEmpty(p.assetBundleVariant))
                    config = bundleList[bundleDict[p.assetBundleName]];
                else
                    config = bundleList[bundleDict[p.assetBundleVariant]];

                if (p.addressableNames != null && p.addressableNames.Length == p.assetNames.Length)
                {
                    for (int i = 0; i < p.assetNames.Length; i++)
                    {
                        string addressName = p.addressableNames[i];
                        if (string.IsNullOrEmpty(addressName))
                            assetList.Add(p.assetNames[i]);
                        else if (string.IsNullOrWhiteSpace(addressName))
                            continue;
                        else
                            assetList.Add(addressName);
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

            return new BundleMgrManifest() { BundleList = bundleList.ToArray() };
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
