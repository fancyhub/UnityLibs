/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH.AssetBundleBuilder.Ed
{
    public class PostBuild_SaveGraph : BuilderPostBuild
    {
        public override void OnPostBuild(PostBuildContext context)
        {
            string content = UnityEngine.JsonUtility.ToJson(context.AssetGraph, true);
            string path = System.IO.Path.Combine(context.Config.GetOutputDir(context.Target), "graph.json");
            System.IO.File.WriteAllText(path, content);
        }
    }
}
