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
            context.AssetGraph.SaveToDir(context.Config.GetOutputDir(context.Target));
        }
    }
}
