/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using FH.AssetBundleBuilder.Ed;
using System.Collections.Generic;
using System.IO;
using FH.Ed;
using FH.FileManagement.Ed;
using System.Linq;

namespace FH.ResManagement.Ed
{
    public class FileBuildStep_BuildBundle : FH.FileManagement.Ed.BuildStep
    {
        public BundleTags Tags = new BundleTags();


        public override void Build(BuildContext context)
        {
            AssetBundleBuilderConfig config = AssetBundleBuilderConfig.GetDefault();
            var result = FH.AssetBundleBuilder.Ed.Builder.Build(config, context.BuildTarget);

            Convert2FileBuildInfo(config.GetOutputDir(context.BuildTarget), result.AssetGraph, Tags, context);
        }


        public static void Convert2FileBuildInfo(string input_dir, AssetGraph asset_graph, BundleTags bundle_tags_config, BuildContext dest_context)
        {
            List<string>[] bundle_tags_list = new List<string>[asset_graph.Bundles.Count];
            for (int i = 0; i < asset_graph.Bundles.Count; i++)
                bundle_tags_list[i] = new List<string>();


            {
                bundle_tags_config.BuildCache();
                HashSet<int> bundle_index_set = new HashSet<int>();

                HashSet<string> temp_tags = new HashSet<string>();
                for (int bundle_index = 0; bundle_index < asset_graph.Bundles.Count; bundle_index++)
                {
                    AssetGraph.Bundle bundle = asset_graph.Bundles[bundle_index];

                    //获取该Bundle下的所有资源对应的tags
                    temp_tags.Clear();
                    foreach (int asset_index in bundle.Assets)
                    {
                        AssetGraph.Asset asset = asset_graph.Assets[asset_index];
                        bundle_tags_config.GetTags(asset.Path, temp_tags);
                    }
                    if (temp_tags.Count == 0)
                        continue;

                    //获取所有依赖的bundles,包括自己
                    bundle_index_set.Clear();
                    bundle_index_set.Add(bundle_index);
                    asset_graph.GetBundleAllDeps(bundle_index, bundle_index_set);


                    //给所有的bundle 添加tags
                    foreach (var p in bundle_index_set)
                    {
                        bundle_tags_list[p].AddRange(temp_tags);
                    }
                }
            }


            {
                HashSet<string> tags = new HashSet<string>();
                for (int i = 0; i < bundle_tags_list.Length; i++)
                {
                    tags.Clear();
                    var list = bundle_tags_list[i];
                    foreach (var p in list)
                    {
                        if (string.IsNullOrEmpty(p))
                            continue;
                        tags.Add(p.Trim());
                    }

                    string path = System.IO.Path.Combine(input_dir, asset_graph.Bundles[i].Name);

                    dest_context.AddFileInfo(path, asset_graph.Bundles[i].FileHash, new List<string>(tags));
                }
            }
        }
    }
}
