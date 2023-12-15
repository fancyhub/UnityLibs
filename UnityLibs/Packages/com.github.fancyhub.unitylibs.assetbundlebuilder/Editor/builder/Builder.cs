/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

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
            BuilderTimer.Intend = 0;
            using var _ = new BuilderTimer("Build Bundles");
                    
            //1. 获取所有可寻址的资源
            List<(string path, string address)> fileList = null;
            using (new BuilderTimer("1/6. Collect"))
            {
                IAssetCollector assetCollection = config.GetAssetCollector();
                fileList = assetCollection.GetAllAssets();
            }


            //2. 添加到 AssetObjMap, 建立依赖关系
            AssetObjMap assetMap = new AssetObjMap();
            using (new BuilderTimer("2/6. Dep"))
            {
                assetMap.SetDepCollection(config.GetAssetDepCollection());
                foreach (var file in fileList)
                {
                    assetMap.AddObject(file.path, file.address);
                }
            }
            //assetmap.CheckCycleDep();

            //3. 分组
            BundleNodeMap bundleMap = new BundleNodeMap();
            using (new BuilderTimer("3/6. Group"))
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


            //4. 检查完整性, 是否 AssetMap里面所有的资源都有对应的包
            using (new BuilderTimer("4/6. Check Group"))
            {
                bundleMap.CheckAllAssetsInBundle(assetMap.GetAllObjects());
            }

            //5. 生成
            AssetBundleBuild[] buildInfoList = null;
            AssetBundleManifest unityManifest = null;
            BuilderParam param = config.GetBuilderParam(target);
            using (new BuilderTimer("5/6. Build Bundles"))
            {
                buildInfoList = bundleMap.GenAssetBundleBuildList();         
                unityManifest = UnityEditor.BuildPipeline.BuildAssetBundles(param.OutputDir, buildInfoList, param.BuildOptions, target);
            }

            //6. 生成自己的Manifest
            using (new BuilderTimer("6/6. Gen My Manifest"))
            {
                var myManifest = FH.BundleMgrManifest.EdCreateFromManifest(unityManifest, buildInfoList);
                string path = System.IO.Path.Combine(param.OutputDir, "manifest.json");
                System.IO.File.WriteAllText(path, UnityEngine.JsonUtility.ToJson(myManifest, true));

                AssetGraph graph = AssetGraph.CreateFrom(bundleMap);
                string path2 = System.IO.Path.Combine(param.OutputDir, "graph.json");
                System.IO.File.WriteAllText(path2, UnityEngine.JsonUtility.ToJson(graph, true));
            }
        }
    }
}
