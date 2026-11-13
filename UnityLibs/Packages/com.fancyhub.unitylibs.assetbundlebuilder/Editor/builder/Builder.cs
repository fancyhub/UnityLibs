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

namespace FH.AssetBundleBuilder.Ed
{
    public static partial class Builder
    {
        [MenuItem("Tools/FancyHub/Res/AssetBundle/Build")]
        public static void Build()
        {
            AssetBundleBuilderConfig config = AssetBundleBuilderConfig.GetDefault();
            Build(config, EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("Tools/FancyHub/Res/AssetBundle/Asset Graph Viewer")]
        public static void ShowAssetGraphWindow()
        {
            EditorWindow.GetWindow(typeof(AssetGraphViewWindow)).Show();
        }

        public static ABBuildContext Build(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target, bool fake = false)
        {
            ABBuildContext context = new ABBuildContext()
            {
                Target = target,
                PipelineType = config.PipelineType,
                BuilderParam = config.GetBuilderParam(target),
                Fake = fake,
                OutputDir = config.GetOutputDir(target),

                AssetGraph = null,
            };

            BuilderTimer.Intend = 0;
            using var _ = new BuilderTimer("Build Bundles");
            int cur_step = 1;
            int total_steps = 9;

            //1. 预处理
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Pre Build"))
            {
                foreach (var p in config.PreBuild)
                {
                    if (p != null && p.Enable)
                        p.OnPreBuild(context);
                }
            }

            //2. 获取所有可寻址的资源
            List<(string path, string address)> fileList = null;
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Collect"))
            {
                IAssetCollector assetCollection = config.GetAssetCollector(context);
                fileList = assetCollection.GetAllAssets(context);
            }

            //3. 添加到 AssetObjMap, 建立依赖关系
            AssetObjMap assetMap = new AssetObjMap();
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Dep"))
            {
                IAssetDependency assetDep = config.GetAssetDependency(context);
                assetMap.SetDepCollection(assetDep);
                foreach (var file in fileList)
                {
                    assetMap.AddObject(file.path, file.address);
                }

                assetDep.OnFinishCollect();
            }
            //assetMap.CheckCycleDep();

            //4. 分组
            BundleNodeMap bundleMap = new BundleNodeMap();
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Group"))
            {
                IBundleRuler bundle_ruler = config.GetBundleRuler(context);
                foreach (var file in fileList)
                {
                    var asset = assetMap.FindObject(file.path);

                    string bundle_name = bundle_ruler.GetBundleName(context, asset.FilePath, asset.AssetType, asset.NeedExport);
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
            FileUtil.CreateDir(context.OutputDir);
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Build Bundles"))
            {
                if (fake)
                {
                    UnityEngine.Debug.Log("Fake Build");
                }
                else
                {
                    switch (config.PipelineType)
                    {
                        default:
                        case EBuildPipeline.Legacy:
                            _BuildABLegacy(context, bundleMap);
                            break;

                        case EBuildPipeline.SBP_Simple:
                            _BuildABSbpSimple(context, bundleMap);
                            break;

                        case EBuildPipeline.SBP_Custom:
                            _BuildABSbpCustom(context, bundleMap);
                            break;
                    }
                }
            }

            //7. 生成 Graph
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Gen Graph"))
            {
                context.AssetGraph = AssetGraph.CreateFromBundleNodeMap(bundleMap);
            }

            //8. tags
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Add Tags"))
            {
                ITagRuler tag_rule = config.GetTagRuler(context);
                _BuildTags(context, tag_rule);
            }

            //9. 后处理
            using (new BuilderTimer($"{cur_step++}/{total_steps}. Post Build"))
            {
                foreach (var p in config.PostBuild)
                {
                    if (p == null || !p.Enable)
                        continue;

                    p.OnPostBuild(context);
                }
                return context;
            }
        }
        private static void _BuildTags(ABBuildContext context, ITagRuler tag_rule)
        {
            List<string> asset_list = new List<string>();
            HashSet<string>[] bundle_tag_array = new HashSet<string>[context.AssetGraph.Bundles.Count];
            for (int i = 0; i < bundle_tag_array.Length; i++)
                bundle_tag_array[i] = new HashSet<string>();

            //给每个bundle 打上标签
            {
                HashSet<string> temp_tags = new HashSet<string>();
                HashSet<int> temp_bundle_index_set = new HashSet<int>();

                foreach (var bundle in context.AssetGraph.Bundles)
                {
                    asset_list.Clear();
                    context.AssetGraph.GetAssetPathListInBundle(bundle.Index, asset_list);

                    temp_tags.Clear();
                    tag_rule.GetTags(context,bundle.Name, asset_list, temp_tags);

                    temp_bundle_index_set.Clear();
                    temp_bundle_index_set.Add(bundle.Index);
                    context.AssetGraph.GetBundleAllDeps(bundle.Index, temp_bundle_index_set);

                    foreach (int bundle_index in temp_bundle_index_set)
                    {
                        foreach (string tag in temp_tags)
                            bundle_tag_array[bundle_index].Add(tag);
                    }
                }
            }

            List<string> tag_list = new List<string>();
            foreach (var bundle in context.AssetGraph.Bundles)
            {
                tag_list.Clear();
                tag_list.AddRange(bundle_tag_array[bundle.Index]);
                tag_list.Sort();
                bundle.Tags = tag_list.ToArray();
            }
        }
    }
}
