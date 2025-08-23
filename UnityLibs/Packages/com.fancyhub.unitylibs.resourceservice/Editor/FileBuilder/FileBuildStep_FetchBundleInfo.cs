/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using FH.AssetBundleBuilder.Ed;
using System.Collections.Generic;
using UnityEngine;
using FH.Ed;
using FH.FileManagement.Ed;

namespace FH.Ed.ResManagement
{
    /// <summary>
    /// 从已经生成好的 assetbundle list 生成 file info list
    /// </summary>
    public class FileBuildStep_FetchBundleInfo : FH.FileManagement.Ed.BuildStep
    {
        public AssetBundleBuilderConfig ABBuilderConfig;
        [Header("eg: tag_a;tag_b")]
        public string BundleManifestTags = "base";

        public override List<BuildFileInfo> Build(BuildContext context)
        {
            string dir = ABBuilderConfig.GetOutputDir(context.BuildTarget);
            string path = System.IO.Path.Combine(dir, AssetGraph.CDefaultName);

            AssetGraph graph = AssetGraph.LoadFromFile(path);

            List<BuildFileInfo> ret = new List<BuildFileInfo>();
            foreach (var p in graph.Bundles)
            {
                string file_path = System.IO.Path.Combine(dir, p.Name);
                ret.Add(new BuildFileInfo()
                {
                    FileName = p.Name,
                    FilePath = file_path,
                    FileHash = p.FileHash,
                    Tags = new List<string>(p.Tags),
                });
            }

            {
                string bundle_manifest_path = System.IO.Path.Combine(dir, BundleManifest.DefaultFileName);
                if (!System.IO.File.Exists(bundle_manifest_path))
                {
                    throw new Exception($"找不到Bundle Manifest: {bundle_manifest_path}");
                }
                ret.Add(new BuildFileInfo()
                {
                    FileName = BundleManifest.DefaultFileName,
                    FilePath = bundle_manifest_path,
                    FileHash = MD5Helper.ComputeFile(bundle_manifest_path),
                    Tags = new List<string>(BundleManifestTags.Split(';', StringSplitOptions.RemoveEmptyEntries)),
                });
            }

            return ret;
        }
    }
}
