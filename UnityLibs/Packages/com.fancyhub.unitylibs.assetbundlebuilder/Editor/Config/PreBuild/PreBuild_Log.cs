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
        public override void OnPreBuild(ABBuildContext context)
        {
            UnityEngine.Debug.Log($"Build {context.Target} in {context.OutputDir}");
        }
    }
}
