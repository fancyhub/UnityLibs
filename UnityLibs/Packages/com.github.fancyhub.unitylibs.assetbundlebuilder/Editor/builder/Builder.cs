/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Content;

namespace FH.AssetBundleBuilder.Ed
{
    public static class Builder
    {
        [MenuItem("Tools/AssetBundle/Build")]
        public static void Build()
        {
            AssetBundleBuilderConfig config = AssetBundleBuilderConfig.GetDefault();
            Build(config, EditorUserBuildSettings.activeBuildTarget);
        }

        public static PostBuildContext Build(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target)
        {
            BuilderParam param = config.GetBuilderParam(target);
            string outputDir = config.GetOutputDir(target);
            BuilderTimer.Intend = 0;
            using var _ = new BuilderTimer("Build Bundles");
            int cur_step = 1;
            int total_steps = 8;

            //1. 预处理
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Pre Build"))
            {
                foreach (var p in config.PreBuild)
                {
                    if (p != null && p.Enable)
                        p.OnPreBuild(config, target);
                }
            }

            //2. 获取所有可寻址的资源
            List<(string path, string address)> fileList = null;
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Collect"))
            {
                IAssetCollector assetCollection = config.GetAssetCollector();
                fileList = assetCollection.GetAllAssets();
            }


            //3. 添加到 AssetObjMap, 建立依赖关系
            AssetObjMap assetMap = new AssetObjMap();
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Dep"))
            {
                assetMap.SetDepCollection(config.GetAssetDependency());
                foreach (var file in fileList)
                {
                    assetMap.AddObject(file.path, file.address);
                }
            }
            //assetmap.CheckCycleDep();

            //4. 分组
            BundleNodeMap bundleMap = new BundleNodeMap();
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Group"))
            {
                IBundleRuler bundle_ruler = config.GetBundleRuler();
                foreach (var file in fileList)
                {
                    var asset = assetMap.FindObject(file.path);

                    string bundle_name = bundle_ruler.GetBundleName(asset.FilePath, asset.AssetType, asset.NeedExport);
                    if (!string.IsNullOrEmpty(bundle_name))
                        bundleMap.Add(asset, bundle_name);
                }
                bundleMap.Build();
            }


            //5. 检查完整性, 是否 AssetMap里面所有的资源都有对应的包
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Check Group"))
            {
                bundleMap.CheckAllAssetsInBundle(assetMap.GetAllObjects());
            }

            //6. 生成
            AssetBundleBuild[] buildInfoList = null;
            AssetBundleManifest unityManifest = null;
            FileUtil.CreateDir(outputDir);
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Build Bundles"))
            {
                buildInfoList = bundleMap.GenAssetBundleBuildList();
                unityManifest = UnityEditor.BuildPipeline.BuildAssetBundles(outputDir, buildInfoList, param.BuildOptions, target);
            }


            //7. 生成 Graph
            AssetGraph graph = null;
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Gen Graph"))
            {
                graph = AssetGraph.CreateFromBundleNodeMap(bundleMap);
                foreach (var p in graph.Bundles)
                {
                    p.FileHash = unityManifest.GetAssetBundleHash(p.Name).ToString();
                }
            }

            //8. tags
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Add Tags"))
            {
                ITagRuler tag_rule = config.GetTagRuler();
                _BuildTags(graph, tag_rule);
            }

            //9. 后处理
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Post Build"))
            {
                PostBuildContext context = new PostBuildContext()
                {
                    Target = target,
                    Config = config,
                    AssetGraph = graph,
                    Manifest = unityManifest,
                    BundleBuildArray = buildInfoList,
                };

                foreach (var p in config.PostBuild)
                {
                    if (p == null || !p.Enable)
                        continue;

                    p.OnPostBuild(context);
                }
                return context;
            }
        }

        private static void _BuildTags(AssetGraph graph, ITagRuler tag_rule)
        {
            List<string> asset_list = new List<string>();
            HashSet<string>[] bundle_tag_array = new HashSet<string>[graph.Bundles.Count];
            for (int i = 0; i < bundle_tag_array.Length; i++)
                bundle_tag_array[i] = new HashSet<string>();

            //给每个bundle 打上标签
            {
                HashSet<string> temp_tags = new HashSet<string>();
                HashSet<int> temp_bundle_index_set = new HashSet<int>();

                foreach (var bundle in graph.Bundles)
                {
                    asset_list.Clear();
                    graph.GetAssetPathListInBundle(bundle.Index, asset_list);

                    temp_tags.Clear();
                    tag_rule.GetTags(bundle.Name, asset_list, temp_tags);

                    temp_bundle_index_set.Clear();
                    temp_bundle_index_set.Add(bundle.Index);
                    graph.GetBundleAllDeps(bundle.Index, temp_bundle_index_set);

                    foreach (int bundle_index in temp_bundle_index_set)
                    {
                        foreach (string tag in temp_tags)
                            bundle_tag_array[bundle_index].Add(tag);
                    }
                }
            }

            List<string> tag_list = new List<string>();
            foreach (var bundle in graph.Bundles)
            {
                tag_list.Clear();
                tag_list.AddRange(bundle_tag_array[bundle.Index]);
                tag_list.Sort();
                bundle.Tags = tag_list.ToArray();
            }
        }
    }
}
