/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using FH.Ed;
using FH.FileManagement.Ed;

namespace FH.ResManagement.Ed
{
    public class FileBuildStep_BuildZip : FH.FileManagement.Ed.BuildStep
    {
        public List<FH.VFSManagement.Builder.BuilderConfig> ZipConfigs = new List<VFSManagement.Builder.BuilderConfig>();
        public string OutputDir = "Bundle/Builder";

        public override void Build(BuildContext context)
        {
            string dest_dir = System.IO.Path.Combine(OutputDir, context.Target2Name());

            foreach (var p in ZipConfigs)
            {
                string dest_file = FH.VFSManagement.Builder.BuilderUtil.BuildZip(p, dest_dir);

                UnityEngine.Debug.Log($"Build Zip {p.Name} -> {dest_file}");

                string file_hash = MD5Helper.ComputeFile(dest_file);
                if (string.IsNullOrEmpty(file_hash))
                    continue;
                context.AddFileInfo(dest_file, file_hash, p.Tags);
            }
        }
    }
}
