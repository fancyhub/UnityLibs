/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using FH.AssetBundleBuilder.Ed;
using System.Collections.Generic;
using FH.FileManagement.Ed;
using UnityEngine;
using FH.Ed;

namespace FH.Ed.ResManagement
{
    /// <summary>
    /// 调用 AssetBundle的builder, 并返回文件列表
    /// </summary>
    public class FileBuildStep_BuildBundle : FH.FileManagement.Ed.BuildStep
    {
        public AssetBundleBuilderConfig ABBuilderConfig;
        [Header("eg: tag_a;tag_b")]
        public string BundleManifestTags = "base";

        public override List<BuildFileInfo> Build(BuildContext context)
        {
            if (ABBuilderConfig == null)
                return null;
            var result = FH.AssetBundleBuilder.Ed.Builder.Build(ABBuilderConfig, context.BuildTarget);

            string input_dir = ABBuilderConfig.GetOutputDir(context.BuildTarget);

            List<BuildFileInfo> ret = new List<BuildFileInfo>();
            foreach (var p in result.AssetGraph.Bundles)
            {
                string file_path = System.IO.Path.Combine(input_dir, p.Name);
                ret.Add(new BuildFileInfo()
                {
                    FileName = p.Name,
                    FilePath = file_path,
                    FileHash = p.FileHash,
                    Tags = new List<string>(p.Tags),
                });
            }

            {
                string bundle_manifest_path = System.IO.Path.Combine(input_dir, BundleManifest.DefaultFileName);
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
