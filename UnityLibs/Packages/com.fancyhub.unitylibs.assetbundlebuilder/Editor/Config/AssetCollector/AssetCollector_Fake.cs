/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// fake
    /// </summary>
    public class AssetCollector_Fake : BuilderAssetCollector
    {
        public string GraphPath = "";
        private List<(string path, string address)> _asset_list;

        public override bool IsFake() { return true; }
        public override List<(string path, string address)> GetAllAssets(ABBuildContext context)
        {
            return _asset_list;
        }

        public override IAssetCollector GetAssetCollector(ABBuildContext context)
        {
            var file_path = GraphPath.Replace("{Target}", context.Target.Ext2DirName());

            var graph = AssetGraph.LoadFromFile(file_path);
            _asset_list = new List<(string path, string address)>(graph.Assets.Count);
            foreach (var asset in graph.Assets)
            {
                if (asset.Export)
                    _asset_list.Add((asset.Path, asset.Address));
            }
            return this;
        }
    }
}
