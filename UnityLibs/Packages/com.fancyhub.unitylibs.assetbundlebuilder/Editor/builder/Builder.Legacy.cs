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
        private static (AssetBundleManifest unityManifest, AssetBundleBuild[] buildInfoList) _BuildABNormal(BuildTarget target, BuilderParam param, string outputDir, BundleNodeMap bundleMap)
        {
            var buildInfoList = bundleMap.GenAssetBundleBuildList();
            var unityManifest = UnityEditor.BuildPipeline.BuildAssetBundles(outputDir, buildInfoList, param.BuildOptions, target);

            foreach (var p in bundleMap.GetAllNodes())
            {
                p.FileHash = unityManifest.GetAssetBundleHash(p.GetNodeName()).ToString();
            }
            return (unityManifest, buildInfoList);
        }
    }
}
