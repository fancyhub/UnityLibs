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

namespace FH.ResManagement.Ed
{
    public class FileBuildStep_FetchBundleInfo : FH.FileManagement.Ed.BuildStep
    {
        public string InputDir = "Bundle/Builder";
        public string AssetGraphFileName = "graph.json";

        public override List<BuildFileInfo> Build(BuildContext context)
        {            
            string dir = System.IO.Path.Combine(InputDir, context.Target2Name());
            string path = System.IO.Path.Combine(dir, AssetGraphFileName);

            AssetGraph graph = AssetGraph.LoadFromFile(path);

            List<BuildFileInfo> ret = new List<BuildFileInfo>();
            foreach (var p in graph.Bundles)
            {
                string file_path = System.IO.Path.Combine(dir, p.Name);
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
