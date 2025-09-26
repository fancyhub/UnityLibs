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
        private static (AssetBundleManifest unityManifest, AssetBundleBuild[] buildInfoList) _BuildABSBP(BuildTarget target, BuilderParam param, string outputDir, BundleNodeMap bundleMap)
        {            
            throw new NotImplementedException();
        }
    }

#else
    public static partial class Builder
    {
        private static (AssetBundleManifest unityManifest, AssetBundleBuild[] buildInfoList) _BuildABSBP(BuildTarget target, BuilderParam param, string outputDir, BundleNodeMap bundleMap)
        {
            var sbp_tasks = _CreateSBPTasks("builtin_shader");
            var sbp_param = param.SBPParam.ToSbpParam(target, outputDir);
            var sbp_content = bundleMap.GenAssetBundleBuildListForSBP();
            var exitCode = UnityEditor.Build.Pipeline.ContentPipeline.BuildAssetBundles(sbp_param, sbp_content, out var buildResults, sbp_tasks);
            if (exitCode != UnityEditor.Build.Pipeline.ReturnCode.Success)
                throw new UnityEditor.Build.BuildFailedException("SBP Build Error: " + exitCode);

            foreach (var p in bundleMap.GetAllNodes())
            {
                string path = System.IO.Path.Combine(outputDir, p.GetNodeName());
                p.FileHash = FileUtil.CalcFileHash(path);
            }

            return (null, bundleMap.GenAssetBundleBuildList());
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

        private static IList<UnityEditor.Build.Pipeline.Interfaces.IBuildTask> _CreateSBPTasks(string builtInShaderBundleName)
        {
            var buildTasks = new List<UnityEditor.Build.Pipeline.Interfaces.IBuildTask>();

            // Setup
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.SwitchToBuildPlatform());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.RebuildSpriteAtlasCache());

            // Player Scripts
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.BuildPlayerScripts());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.PostScriptsCallback());

            // Dependency
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.CalculateSceneDependencyData());
#if UNITY_2019_3_OR_NEWER
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.CalculateCustomDependencyData());
#endif
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.CalculateAssetDependencyData());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.StripUnusedSpriteSources());
            //buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.CreateBuiltInShadersBundle(builtInShaderBundleName));
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.PostDependencyCallback());

            // Packing
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.GenerateBundlePacking());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.UpdateBundleObjectLayout());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.GenerateBundleCommands());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.GenerateSubAssetPathMaps());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.GenerateBundleMaps());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.PostPackingCallback());

            // Writing
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.WriteSerializedFiles());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.ArchiveAndCompressBundles());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.AppendBundleHash());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.GenerateLinkXml());
            buildTasks.Add(new UnityEditor.Build.Pipeline.Tasks.PostWritingCallback());

            return buildTasks;
        }
    }

    internal class BundleBuildContentSBP : UnityEditor.Build.Pipeline.Interfaces.IBundleBuildContent
    {
        /// <inheritdoc />
        public List<UnityEditor.GUID> Assets { get; private set; }

        /// <inheritdoc />
        public List<UnityEditor.GUID> Scenes { get; private set; }

#if UNITY_2019_3_OR_NEWER
        /// <inheritdoc />
        public List<UnityEditor.Build.Pipeline.Interfaces.CustomContent> CustomAssets { get; private set; }

        /// <inheritdoc />
        public Dictionary<string, List<UnityEditor.Build.Content.ResourceFile>> AdditionalFiles { get; private set; }
#endif

        /// <inheritdoc />
        public Dictionary<UnityEditor.GUID, string> Addresses { get; private set; }

        /// <inheritdoc />
        public Dictionary<string, List<UnityEditor.GUID>> BundleLayout { get; private set; }

        public BundleBuildContentSBP(int asset_count, int bundle_count)
        {
            this.Assets = new List<UnityEditor.GUID>(asset_count);
            this.Scenes = new List<UnityEditor.GUID>(asset_count);
            this.CustomAssets = new List<UnityEditor.Build.Pipeline.Interfaces.CustomContent>();
            this.AdditionalFiles = new Dictionary<string, List<UnityEditor.Build.Content.ResourceFile>>();
            this.Addresses = new Dictionary<UnityEditor.GUID, string>(asset_count);
            this.BundleLayout = new Dictionary<string, List<UnityEditor.GUID>>(bundle_count);
        }
    }
    public partial class BundleNode
    {
        internal void GenAssetBundleBuildForSBP(BundleBuildContentSBP content)
        {
            List<UnityEditor.GUID> layout_assets = new List<UnityEditor.GUID>(_main_objs.Count + _dep_objs.Count);
            content.BundleLayout.Add(GetNodeName(), layout_assets);
            List<UnityEditor.GUID> assets = IsSceneNode() ? content.Scenes : content.Assets;
            Dictionary<UnityEditor.GUID, string> address = content.Addresses;

            foreach (AssetObj obj in _main_objs)
            {
                var guid = new UnityEditor.GUID(obj.GUID);
                assets.Add(guid);
                layout_assets.Add(guid);
                if (obj.NeedExport)
                    address.Add(guid, obj.AddressName);
                else
                    address.Add(guid, obj.GUID);
            }

            foreach (AssetObj obj in _dep_objs)
            {
                var guid = new UnityEditor.GUID(obj.GUID);
                assets.Add(guid);
                layout_assets.Add(guid);
                if (obj.NeedExport)
                    address.Add(guid, obj.AddressName);
                else
                    address.Add(guid, obj.GUID);
            }
        }
    }

    public partial class BundleNodeMap
    {
        public UnityEditor.Build.Pipeline.Interfaces.IBundleBuildContent GenAssetBundleBuildListForSBP()
        {
            HashSet<BundleNode> node_list = _nodes_map;
            if (node_list.Count == 0)
            {
                throw new System.Exception("没有任何资源被打包");
            }

            BundleBuildContentSBP ret = new BundleBuildContentSBP(_node_add.AssetCount, node_list.Count);
            foreach (BundleNode node in node_list)
            {
                node.GenAssetBundleBuildForSBP(ret);
            }
            return ret;
        }         
    }
#endif
}
