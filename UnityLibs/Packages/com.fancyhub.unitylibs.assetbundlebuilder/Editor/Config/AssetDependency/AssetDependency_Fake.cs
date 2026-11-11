/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 依赖收集工具
    /// </summary>
    public class AssetDependency_Fake : BuilderAssetDependency, IAssetDependency
    {
        public string GraphPath = "";

        private List<AssetGraph.Asset> _Assets;
        private Dictionary<string, AssetGraph.Asset> _DictAssets;

        public override bool IsFake()
        {
            return true;
        }

        public override IAssetDependency GetAssetDependency()
        {
            AssetGraph graph = AssetGraph.LoadFromFile(GraphPath);
            _Assets = graph.Assets;
            _DictAssets = new Dictionary<string, AssetGraph.Asset>(_Assets.Count);
            foreach (var asset in _Assets)
                _DictAssets[asset.Path] = asset;
            return this;
        }

        public override List<string> CollectDirectDeps(string asset_path, EAssetObjType asset_type)
        {
            if (_DictAssets == null)
                return new List<string>();

            if (!_DictAssets.TryGetValue(asset_path, out var asset))
                return new List<string>();

            List<string> deps = new List<string>(asset.Deps.Length);
            foreach (var p in asset.Deps)
            {
                deps.Add(_Assets[p].Path);
            }
            return deps;
        }

        public override string FileGuid(string path)
        {
            if (_DictAssets == null)
                return "no_guid";

            if (!_DictAssets.TryGetValue(path, out var asset))
                return "no_guid";

            return asset.GUID;
        }


        public override void OnFinishCollect()
        {
        }
    }
}
