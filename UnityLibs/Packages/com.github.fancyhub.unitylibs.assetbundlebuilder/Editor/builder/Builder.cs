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
            //1. 获取所有可寻址的资源
            IAssetCollection assetCollection = config.GetAssetCollection();
            List<(string path, string address)> fileList = assetCollection.GetAllAssets();

            //2. 添加到 AssetObjMap, 建立依赖关系
            AssetObjMap assetMap = new AssetObjMap();
            assetMap.SetDepCollection(config.GetAssetDepCollection());
            foreach (var file in fileList)
            {
                assetMap.AddObject(file.path, file.address);
            }
            //assetmap.CheckCycleDep();

            //3. 分组
            BundleNodeMap bundleMap = new BundleNodeMap();
            foreach (var file in fileList)
            {
                var asset = assetMap.FindObject(file.path);
                bundleMap.Add(asset, "test");
            }
            bundleMap.Build();

            //4. 检查完整性, 是否 AssetMap里面所有的资源都有对应的包
            bundleMap.CheckAllAssetsInBundle(assetMap.GetAllObjects());

            BuilderParam param = config.GetBuilderParam(target);
            //5. 生成
            AssetBundleBuild[] buildInfoList = bundleMap.GenAssetBundleBuildList();
            var manifest = UnityEditor.BuildPipeline.BuildAssetBundles(param.OutputDir, buildInfoList, param.BuildOptions, target);

            //6. 生成自己的Manifest
            var myManifest = FH.BundleMgrManifest.EdCreateFromManifest(manifest, buildInfoList);
            string path = System.IO.Path.Combine(param.OutputDir, "manifest.json");
            System.IO.File.WriteAllText(path, UnityEngine.JsonUtility.ToJson(myManifest, true));
        }
    }
}
