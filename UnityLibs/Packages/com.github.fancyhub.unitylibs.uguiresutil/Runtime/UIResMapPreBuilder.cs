/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/19
 * Title   : 
 * Desc    : 
*************************************************************************************/
#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UI;
using FH.AssetBundleBuilder.Ed;

namespace FH.UI
{
    public class PreBuild_UIResMap: BuilderPreBuild
    {
        public override void OnPreBuild(AssetBundleBuilderConfig config, UnityEditor.BuildTarget target)
        {
            UnityEditor.AssetDatabase.MakeEditable(UIResMapConfig.CEditorPATH);
            Debug.Log("UIResMap 生成开始===========================");
            UIResMapConfig asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UIResMapConfig>(UIResMapConfig.CEditorPATH);
            if (!asset.Config.EdCollectAll())
            {
                throw new UnityEditor.Build.BuildFailedException("Build Res Map Faild, 查看前面的错误");
            }

            UnityEditor.AssetDatabase.ForceReserializeAssets(new string[] { UIResMapConfig.CEditorPATH }, UnityEditor.ForceReserializeAssetsOptions.ReserializeAssets);

        }
    }
}

#endif