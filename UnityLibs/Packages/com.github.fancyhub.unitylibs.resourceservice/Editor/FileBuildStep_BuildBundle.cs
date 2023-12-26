/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using FH.AssetBundleBuilder.Ed;
using System.Collections.Generic;
using System.IO;
using FH.Ed;
using FH.FileManagement.Ed;
using System.Linq;

namespace FH.ResManagement.Ed
{
    public class FileBuildStep_BuildBundle : FH.FileManagement.Ed.BuildStep
    {
        public override List<BuildFileInfo> Build(BuildContext context)
        {
            AssetBundleBuilderConfig config = AssetBundleBuilderConfig.GetDefault();
            var result = FH.AssetBundleBuilder.Ed.Builder.Build(config, context.BuildTarget);

            string input_dir = config.GetOutputDir(context.BuildTarget);

            List<BuildFileInfo> ret = new List<BuildFileInfo>();
            foreach (var p in result.AssetGraph.Bundles)
            {
                string file_path = System.IO.Path.Combine(input_dir, p.Name);
                ret.Add(new BuildFileInfo()
                {
                    FilePath = file_path,
                    FileHash = p.FileHash,
                    Tags = new List<string>(p.Tags),
                });
            }
            return ret;
        }         
    }
}
