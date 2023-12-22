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

        public static void Build(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target)
        {
            BuilderParam param = config.GetBuilderParam(target);
            string outputDir = config.GetOutputDir(target);
            BuilderTimer.Intend = 0;
            using var _ = new BuilderTimer("Build Bundles");

            //1. 预处理
            using (new BuilderTimer("1/7. Pre Build"))
            {
                foreach (var p in config.PreBuild)
                {
                    if (p != null)
                        p.OnPreBuild(config, target);
                }
            }

            //2. 获取所有可寻址的资源
            List<(string path, string address)> fileList = null;
            using (new BuilderTimer("2/7. Collect"))
            {
                IAssetCollector assetCollection = config.GetAssetCollector();
                fileList = assetCollection.GetAllAssets();
            }


            //3. 添加到 AssetObjMap, 建立依赖关系
            AssetObjMap assetMap = new AssetObjMap();
            using (new BuilderTimer("3/7. Dep"))
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
            using (new BuilderTimer("4/7. Group"))
            {
                IBundleRuler bundle_ruler = config.GetBundleRuler();
                foreach (var file in fileList)
                {
                    var asset = assetMap.FindObject(file.path);

                    string bundle_name = bundle_ruler.GetBundleName(asset.FilePath, asset.AssetType, asset.NeedExport);
                    if (bundle_name != null)
                        bundleMap.Add(asset, bundle_name);
                }
                bundleMap.Build();
            }


            //5. 检查完整性, 是否 AssetMap里面所有的资源都有对应的包
            using (new BuilderTimer("5/7. Check Group"))
            {
                bundleMap.CheckAllAssetsInBundle(assetMap.GetAllObjects());
            }

            //6. 生成
            AssetBundleBuild[] buildInfoList = null;
            AssetBundleManifest unityManifest = null;
            FileUtil.CreateDir(outputDir);
            using (new BuilderTimer("6/7. Build Bundles"))
            {
                buildInfoList = bundleMap.GenAssetBundleBuildList();
                unityManifest = UnityEditor.BuildPipeline.BuildAssetBundles(outputDir, buildInfoList, param.BuildOptions, target);
            }

            //8. 后处理
            using (new BuilderTimer("7/7. Post Build"))
            {
                AssetGraph graph = AssetGraph.CreateFrom(bundleMap);
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
                    if (p == null)
                        continue;

                    p.OnPostBuild(context);
                }
            }
        }

    }
}
