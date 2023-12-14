using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.AssetBundleManager.Builder
{
    public class BuildParams
    {
        public IAssetCollection AssetCollection;
        public IAssetDepCollection AssetDepCollection;
        public BuildAssetBundleOptions BuildOptions = //BuildAssetBundleOptions.UncompressedAssetBundle
                                            BuildAssetBundleOptions.ChunkBasedCompression
                                           | BuildAssetBundleOptions.StrictMode;
        public string OutputDir;
    }

    public static class BundleBuilder
    {
        [MenuItem("GameTool/Build Package")]
        public static void Build()
        {
            BuildParams param = new BuildParams();
            param.AssetCollection = new AssetCollection("Assets/Resources");
            param.AssetDepCollection = new AssetDepCollection();

            Build(param, BuildTarget.StandaloneWindows64);
        }

        public static void Build(BuildParams param, UnityEditor.BuildTarget target)
        {
            //1. 添加
            List<(string path, string address)> fileList = param.AssetCollection.GetAllAssets();

            AssetObjectMap assetmap = new AssetObjectMap();
            assetmap.SetDepCollection(new AssetDepCollection());
            foreach (var file in fileList)
            {
                assetmap.AddObject(file.path, file.address);
            }
            //assetmap.CheckCycleDep();

            //3. 包
            BundleNodeMap bundleMap = new BundleNodeMap();
            foreach (var file in fileList)
            {
                var asset = assetmap.FindObject(file.path);
                bundleMap.Add(asset, "test");
            }
            bundleMap.Build();

            //4. 检查完整性
            bundleMap.CheckAllAssetsInBundle(assetmap.GetAllObjects());

            //5. 生成
            AssetBundleBuild[] buildInfoList = bundleMap.GenAssetBundleBuildList();

            var manifest = UnityEditor.BuildPipeline.BuildAssetBundles("BundleCache", buildInfoList, param.BuildOptions, target);

            var myManifest = FH.BundleMgrManifest.EdCreateFromManifest(manifest, buildInfoList);

            System.IO.File.WriteAllText("BundleCache/manifest.json", UnityEngine.JsonUtility.ToJson(myManifest, true));
        }
    }
}
