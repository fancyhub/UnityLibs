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
    public class PreBuild_AssetsInputConfigBuildCache : BuilderPreBuild
    {
        public AssetsInputConfig Config;
        public override void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target)
        {
            Config?.BuildCache();
        }
    }
}
