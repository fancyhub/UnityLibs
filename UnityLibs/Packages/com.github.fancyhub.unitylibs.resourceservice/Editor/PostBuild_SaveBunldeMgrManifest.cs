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

namespace FH.Res.Ed
{
    public class PostBuild_SaveBunldeMgrManifest : BuilderPostBuild
    {
        public override void OnPostBuild(PostBuildContext context)
        {
            //BundleMgrManifest manifest = BundleMgrManifest.EdCreateFromManifest(context.Manifest, context.BundleBuildArray);
            BundleMgrManifest manifest = _CreateFromGraph(context.AssetGraph);

            string path = System.IO.Path.Combine(context.BuildParam.OutputDir, "manifest.json");
            System.IO.File.WriteAllText(path, UnityEngine.JsonUtility.ToJson(manifest, true));
        }

        private BundleMgrManifest _CreateFromGraph(AssetGraph graph)
        {
            BundleMgrManifest ret = new BundleMgrManifest();

            List<BundleMgrManifest.BundleManifest> bundles = new List<BundleMgrManifest.BundleManifest>();
            List<string> assets = new List<string>();

            foreach (var p in graph.Bundles)
            {
                BundleMgrManifest.BundleManifest bundle = new BundleMgrManifest.BundleManifest();
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
