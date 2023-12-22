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
    public class PreBuild_Log : BuilderPreBuild
    {
        public override void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target)
        {
            UnityEngine.Debug.Log($"Build {target} in {config.GetOutputDir(target)}");
        }
    }
}
