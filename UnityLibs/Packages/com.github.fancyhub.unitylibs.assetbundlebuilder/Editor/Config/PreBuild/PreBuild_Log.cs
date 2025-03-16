/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEditor;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// 预处理, 打印log
    /// </summary>
    public class PreBuild_Log : BuilderPreBuild
    {
        public override void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target)
        {
            UnityEngine.Debug.Log($"Build {target} in {config.GetOutputDir(target)}");
        }
    }
}
