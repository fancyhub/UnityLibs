/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 后处理, 保存 manifest 到graph文件格式
    /// </summary>
    public class PostBuild_SaveGraph : BuilderPostBuild
    {
        public override void OnPostBuild(ABBuildContext context)
        {
            string name = context.Fake ? "graph_fake.json" : AssetGraph.CDefaultName;
            context.AssetGraph.SaveToDir(context.OutputDir, name);
        }
    }
}
