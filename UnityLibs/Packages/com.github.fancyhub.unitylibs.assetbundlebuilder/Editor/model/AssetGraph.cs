/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.AssetBundleBuilder.Ed
{
    [Serializable]
    public sealed class AssetGraph
    {
        public const string CDefaultName = "graph.json";

        public List<Asset> Assets = new List<Asset>();
        public List<Bundle> Bundles = new List<Bundle>();

        [Serializable]
        public sealed class Asset
        {
            public int Index;
            public bool Export;
            public string Path;
            public string Address;
            public int[] Deps;

            public string GetAddressableName()
            {
                if (!Export)
                    return null;

                if (!string.IsNullOrEmpty(Address))
                    return Address;
                return Path;
            }
        }

        [Serializable]
        public sealed class Bundle
        {
            public int Index;
            public string Name;
            public string FileHash;
            public int[] Deps;
            public int[] Assets;
            public string[] Tags;
        }


        public void SaveToDir(string dir)
        {
            string path = System.IO.Path.Combine(dir, CDefaultName);
            string content = UnityEngine.JsonUtility.ToJson(this, true);
            System.IO.File.WriteAllText(path, content);
        }

        public void GetAssetPathListInBundle(int bundle_index, List<string> out_asset_path_list)
        {
            Bundle bundle = Bundles[bundle_index];
            foreach (var p in bundle.Assets)
            {
                out_asset_path_list.Add(Assets[p].Path);
            }
        }

        public void GetBundleAllDeps(int bundle_index, HashSet<int> out_bundle_index_set)
        {
            Bundle bundle = Bundles[bundle_index];
            if (bundle.Deps == null)
                return;

            foreach (var sub in bundle.Deps)
            {
                if (!out_bundle_index_set.Add(sub))
                {
                    GetBundleAllDeps(sub, out_bundle_index_set);
                }
            }
        }

        public static AssetGraph CreateFromBundleNodeMap(BundleNodeMap bundle_map)
        {
            HashSet<AssetObj> assets_set = bundle_map.GetAllAssetObjects();
            List<Asset> assets_list = new List<Asset>(assets_set.Count);
            Dictionary<string, Asset> asset_dict = new Dictionary<string, Asset>(assets_set.Count);

            //1. 建立Asset索引
            {
                int index = 0;
                foreach (var p in assets_set)
                {
                    var asset = new Asset()
                    {
                        Index = index,
                        Path = p.FilePath,
                        Export = p.NeedExport,
                        Address = p.AddressName
                    };
                    index++;
                    asset_dict.Add(asset.Path, asset);
                    assets_list.Add(asset);
                }
            }

            //2. 建立Asset依赖
            {
                List<int> temp_deps = new List<int>();

                foreach (var p in assets_set)
                {
                    temp_deps.Clear();
                    foreach (var p2 in p.GetDirectDepObjs())
                    {
                        asset_dict.TryGetValue(p2.FilePath, out Asset asset);
                        if (asset == null)
                        {
                            throw new Exception($"找不到Asset {p2.FilePath}");
                        }
                        temp_deps.Add(asset.Index);
                    }

                    asset_dict.TryGetValue(p.FilePath, out Asset self);
                    if (self == null)
                    {
                        throw new Exception($"找不到Asset {p.FilePath}");
                    }
                    self.Deps = temp_deps.ToArray();
                }
            }


            HashSet<BundleNode> nodes_set = bundle_map.GetAllNodes();
            List<Bundle> bundles_list = new List<Bundle>(nodes_set.Count);
            Dictionary<string, Bundle> bundle_dict = new Dictionary<string, Bundle>(nodes_set.Count);

            //3. 建立 Bundle 索引
            {
                List<int> temp_assets = new List<int>();
                int index = 0;
                foreach (var p in nodes_set)
                {
                    Bundle bundle = new Bundle()
                    {
                        Index = index,
                        Name = p.GetNodeName(),
                    };

                    bundles_list.Add(bundle);
                    index++;
                    bundle_dict.Add(bundle.Name, bundle);

                    temp_assets.Clear();
                    foreach (var p2 in p)
                    {
                        asset_dict.TryGetValue(p2.FilePath, out Asset asset);
                        if (asset == null)
                        {
                            throw new Exception($"找不到Asset {p2.FilePath}");
                        }
                        temp_assets.Add(asset.Index);
                    }
                    bundle.Assets = temp_assets.ToArray();
                }
            }

            //4. 建立Bundle 依赖
            {
                List<int> temp_deps = new List<int>();
                foreach (var p in nodes_set)
                {
                    temp_deps.Clear();
                    foreach (var p2 in p.GetDepNodes())
                    {
                        bundle_dict.TryGetValue(p2.GetNodeName(), out var bundle);
                        if (bundle == null)
                        {
                            throw new Exception($"找不到Bundle {p2.GetNodeName()}");
                        }
                        temp_deps.Add(bundle.Index);
                    }
                    bundle_dict.TryGetValue(p.GetNodeName(), out var self);
                    if (self == null)
                    {
                        throw new Exception($"找不到Bundle {p.GetNodeName()}");
                    }
                    self.Deps = temp_deps.ToArray();
                }
            }

            return new AssetGraph()
            {
                Assets = assets_list,
                Bundles = bundles_list,
            };
        }


        public static AssetGraph LoadFromFile(string file_path)
        {
            string content = System.IO.File.ReadAllText(file_path);
            return UnityEngine.JsonUtility.FromJson<AssetGraph>(content);
        }
    }
}
