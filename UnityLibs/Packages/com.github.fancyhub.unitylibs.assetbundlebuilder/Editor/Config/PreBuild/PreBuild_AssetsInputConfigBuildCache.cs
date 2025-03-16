/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/26
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEditor;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 生成 config的cache, 该config会收集所有要export出来的文件列表
    /// </summary>
    public class PreBuild_AssetsInputConfigBuildCache : BuilderPreBuild
    {
        public AssetsInputConfig Config;
        public override void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target)
        {
            Config?.BuildCache();
        }
    }
}
