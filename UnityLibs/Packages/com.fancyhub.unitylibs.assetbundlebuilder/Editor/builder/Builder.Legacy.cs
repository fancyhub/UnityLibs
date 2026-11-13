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
        private static void _BuildABLegacy(ABBuildContext context, BundleNodeMap bundleMap)
        {
            var buildInfoList = bundleMap.GenAssetBundleBuildList_Legacy();
            var unityManifest = UnityEditor.BuildPipeline.BuildAssetBundles(context.OutputDir, buildInfoList, context.BuilderParam.BuildOptions, context.Target);

            foreach (var p in bundleMap.GetAllNodes())
            {
                p.FileHash = unityManifest.GetAssetBundleHash(p.GetNodeName()).ToString();
            }
            
        }
    }


    public partial class BundleNodeMap
    {
        public UnityEditor.AssetBundleBuild[] GenAssetBundleBuildList_Legacy()
        {
            HashSet<BundleNode> node_list = _nodes_map;
            if (node_list.Count == 0)
            {
                throw new System.Exception("没有任何资源被打包");
            }

            List<UnityEditor.AssetBundleBuild> builder_list = new List<UnityEditor.AssetBundleBuild>(node_list.Count);
            foreach (BundleNode node in node_list)
            {
                UnityEditor.AssetBundleBuild builder = node.GenAssetBundleBuild_Legacy();

                if (builder.assetBundleName == null || builder.assetNames.Length == 0)
                {
                    throw new Exception("有node 为空 " + node.GetNodeName());
                }
                builder_list.Add(builder);
            }
            return builder_list.ToArray();
        }
    }

    public partial class BundleNode
    {
        public UnityEditor.AssetBundleBuild GenAssetBundleBuild_Legacy()
        {
            UnityEditor.AssetBundleBuild ret = new UnityEditor.AssetBundleBuild();
            ret.assetBundleName = GetNodeName();
            ret.assetNames = _GetAllFiles().ToArray();
            ret.addressableNames = _GetAllAddressNames_Legacy().ToArray();
            return ret;
        }

        private List<string> _GetAllAddressNames_Legacy()
        {
            _SortAssets();
            List<string> file_list = new List<string>(_main_objs.Count + _dep_objs.Count);
            foreach (AssetObj a in _main_objs)
            {
                if (!a.NeedExport)
                    file_list.Add(CIgnoreAddressName);
                else
                    file_list.Add(a.AddressName);
            }

            foreach (AssetObj a in _dep_objs)
            {
                if (!a.NeedExport)
                    file_list.Add(CIgnoreAddressName);
                else
                    file_list.Add(a.AddressName);
            }
            return file_list;
        }
    }
}
