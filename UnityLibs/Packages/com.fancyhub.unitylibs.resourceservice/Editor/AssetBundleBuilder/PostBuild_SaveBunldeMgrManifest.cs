/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using FH.AssetBundleBuilder.Ed;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace FH.Ed.AssetBundleBuilder
{
    /// <summary>
    /// 后处理, 创建 bundle manifest, 不是unity自带的格式
    /// </summary>
    public class PostBuild_SaveBunldeManifest : BuilderPostBuild
    {
        public override void OnPostBuild(PostBuildContext context)
        {
            //BundleMgrManifest manifest = BundleMgrManifest.EdCreateFromManifest(context.Manifest, context.BundleBuildArray);
            BundleManifest manifest = _CreateFromGraph(context.AssetGraph);

            string path = System.IO.Path.Combine(context.Config.GetOutputDir(context.Target), FH.BundleManifest.DefaultFileName);
            System.IO.File.WriteAllText(path, UnityEngine.JsonUtility.ToJson(manifest, true));
        }

        private BundleManifest _CreateFromGraph(AssetGraph graph)
        {
            BundleManifest ret = new BundleManifest();

            List<BundleManifest.Item> bundles = new List<BundleManifest.Item>();
            List<string> assets = new List<string>();

            foreach (var p in graph.Bundles)
            {
                BundleManifest.Item bundle = new BundleManifest.Item();
                bundle.Name = p.Name;

                assets.Clear();
                foreach (var p2 in p.Assets)
                {
                    string address = graph.Assets[p2].GetAddressableName();
                    if (address == null)
                        continue;
                    assets.Add(address);
                }
                bundle.Assets = assets.ToArray();
                bundle.Deps = p.Deps;
                bundles.Add(bundle);
            }

            ret.BundleList = bundles.ToArray();
            return ret;
        }
    }
}
