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
        public BundleTags Tags = new BundleTags();
        public string InputDir = "Bundle/Builder";
        public string AssetGraphFileName = "graph.json";

        public override void Build(BuildContext context)
        {
            string dir = System.IO.Path.Combine(InputDir, context.Target2Name());
            string path = System.IO.Path.Combine(dir, AssetGraphFileName);

            AssetGraph graph = AssetGraph.LoadFromFile(path);

            FileBuildStep_BuildBundle.Convert2FileBuildInfo(dir, graph, Tags, context);
        }
    }
}
