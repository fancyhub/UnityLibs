/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using FH.AssetBundleBuilder.Ed;

namespace FH.Res.Ed
{
    public class PostBuild_SaveBunldeMgrManifest : BuilderPostBuild
    {
        public override void OnPostBuild(PostBuildContext context)
        {
            var myManifest = FH.BundleMgrManifest.EdCreateFromManifest(context.Manifest, context.BundleBuildArray);
            string path = System.IO.Path.Combine(context.BuildParam.OutputDir, "manifest.json");
            System.IO.File.WriteAllText(path, UnityEngine.JsonUtility.ToJson(myManifest, true));
        }
    }
}
