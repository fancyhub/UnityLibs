using System;
using System.Collections.Generic;

namespace FH.AssetBundleBuilder.Ed
{
    [Serializable]
    public class AssetGraph
    {
        public List<Asset> Assets = new List<Asset>();
        public List<Bundle> Bundles = new List<Bundle>();

        [Serializable]
        public class Asset
        {
            public int Id;
            public string Path;
            public string Address;
            public int[] Deps;
        }

        [Serializable]
        public class Bundle
        {
            public int Id;
            public string Name;
            public int[] Deps;
            public int[] Assets;
        }

        public static AssetGraph CreateFrom(BundleNodeMap bundle_map)
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
                        Id = index,
                        Path = p.FilePath,
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
                        temp_deps.Add(asset.Id);
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
                        Id = index,
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
                        temp_assets.Add(asset.Id);
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
                        temp_deps.Add(bundle.Id);
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
    }
}
