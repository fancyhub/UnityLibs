/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace FH.AssetBundleBuilder.Ed
{
#if !ENABLE_SBP
    public static partial class Builder
    {
        private static void _BuildABSbpSimple(BuildTarget target, BuilderParam param, string outputDir, BundleNodeMap bundleMap)
        {            
            throw new NotImplementedException();
        }
    }
#else

    public static partial class Builder
    {
        private static void  _BuildABSbpSimple(BuildTarget target, BuilderParam param, string outputDir, BundleNodeMap bundleMap)
        {
            var sbp_tasks = UnityEditor.Build.Pipeline.DefaultBuildTasks.Create(UnityEditor.Build.Pipeline.DefaultBuildTasks.Preset.AssetBundleCompatible);


            var sbp_content = bundleMap.GenBundleBuildContent_SbpSimple();

            var sbp_param = param.SBPParam.ToSbpParam(target, outputDir);
            var exitCode = UnityEditor.Build.Pipeline.ContentPipeline.BuildAssetBundles(
                sbp_param, 
                sbp_content, 
                out var buildResults,
                sbp_tasks);

            if (exitCode != UnityEditor.Build.Pipeline.ReturnCode.Success)
                throw new UnityEditor.Build.BuildFailedException("SBP Build Error: " + exitCode);

            foreach (var p in bundleMap.GetAllNodes())
            {
                string path = System.IO.Path.Combine(outputDir, p.GetNodeName());
                if (System.IO.File.Exists(path))
                    p.FileHash = FileUtil.CalcFileHash(path);
            }

        }
    }



    public partial class BundleNodeMap
    {
        public UnityEditor.Build.Pipeline.Interfaces.IBundleBuildContent GenBundleBuildContent_SbpSimple()
        {
            HashSet<BundleNode> node_list = _nodes_map;
            if (node_list.Count == 0)
            {
                throw new System.Exception("没有任何资源被打包");
            }

            List<UnityEditor.AssetBundleBuild> builder_list = new List<UnityEditor.AssetBundleBuild>(node_list.Count);
            foreach (BundleNode node in node_list)
            {
                UnityEditor.AssetBundleBuild builder = node.GenAssetBundleBuild_SbpSimple();

                if (builder.assetBundleName == null || builder.assetNames.Length == 0)
                {
                    throw new Exception("有node 为空 " + node.GetNodeName());
                }
                builder_list.Add(builder);
            }
            return new UnityEditor.Build.Pipeline.BundleBuildContent(builder_list.ToArray());
        }
    }

    public partial class BundleNode
    {
        public UnityEditor.AssetBundleBuild GenAssetBundleBuild_SbpSimple()
        {
            UnityEditor.AssetBundleBuild ret = new UnityEditor.AssetBundleBuild();
            ret.assetBundleName = GetNodeName();
            ret.assetNames = _GetAllFiles().ToArray();
            ret.addressableNames = _GetAllAddressNames_SbpSimple().ToArray();
            return ret;
        }

        private List<string> _GetAllAddressNames_SbpSimple()
        {
            _SortAssets();
            List<string> file_list = new List<string>(_main_objs.Count + _dep_objs.Count);
            foreach (AssetObj a in _main_objs)
            {
                if (!a.NeedExport)
                    file_list.Add(a.FilePath);
                else
                    file_list.Add(a.FilePath);
            }

            foreach (AssetObj a in _dep_objs)
            {
                if (!a.NeedExport)
                    file_list.Add(a.FilePath);
                else
                    file_list.Add(a.FilePath);
            }
            return file_list;
        }
    } 


    public partial class BuilderParam
    {
        public partial class SBPParamConfig
        {
            public UnityEditor.Build.Pipeline.BundleBuildParameters ToSbpParam(BuildTarget target, string output_dir)
            {
                var target_group = UnityEditor.BuildPipeline.GetBuildTargetGroup(target);
                var ret = new UnityEditor.Build.Pipeline.BundleBuildParameters(target, target_group, output_dir);

                switch (Compression)
                {
                    default:
                        ret.BundleCompression = UnityEngine.BuildCompression.Uncompressed;
                        break;
                    case CompressionType.Lzma:
                        ret.BundleCompression = UnityEngine.BuildCompression.LZMA;
                        break;
                    case CompressionType.Lz4:
                    case CompressionType.Lz4HC:
                        ret.BundleCompression = UnityEngine.BuildCompression.LZ4;
                        break;
                }
                ret.ContentBuildFlags = BuildFlags;
                ret.UseCache = UseCache;
                ret.CacheServerHost = CacheServerHost;
                ret.CacheServerPort = CacheServerPort;
                ret.WriteLinkXML = WriteLinkXML;
                ret.ScriptOptions = ScriptOptions;
                ret.AppendHash = false;

                return ret;
            }
        }
    }

#endif
}
