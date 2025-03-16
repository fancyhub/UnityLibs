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
    /// <summary>
    /// 生成自己的zip格式的 vfs, 并返回文件列表
    /// </summary>
    public class FileBuildStep_BuildVfs : FH.FileManagement.Ed.BuildStep
    {
        public FH.VFSManagement.Builder.BuilderConfig BuilderConfig;

        public string OutputDir = "ProjTemp/Build/Output/{Target}";

        public override List<BuildFileInfo> Build(BuildContext context)
        {
            if (BuilderConfig == null)
                return null;

            List<BuildFileInfo> ret = new List<BuildFileInfo>();
            string dest_dir = OutputDir.Replace("{Target}", context.BuildTarget.Ext2Name());

            foreach (var p in BuilderConfig.Items)
            {
                string dest_file = p.EdBuildZip(dest_dir);

                UnityEngine.Debug.Log($"Build Zip {p.Name} -> {dest_file}");

                string file_hash = MD5Helper.ComputeFile(dest_file);
                if (string.IsNullOrEmpty(file_hash))
                    continue;
                ret.Add(new BuildFileInfo()
                {
                    FileName = p.Name,
                    FilePath = dest_file,
                    FileHash = file_hash,
                    Tags = new List<string>(p.Tags.Split(';', StringSplitOptions.RemoveEmptyEntries)),
                });
            }
            return ret;
        }
    }
}
