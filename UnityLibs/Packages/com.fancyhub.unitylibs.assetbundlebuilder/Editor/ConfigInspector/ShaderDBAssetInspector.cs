/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FH.Ed;

namespace FH.AssetBundleBuilder.Ed
{

    [CustomEditor(typeof(ShaderDBAsset))]
    public class ShaderDBAssetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UnityEngine.GUILayout.Button("Collect All Materials"))
            {
                ShaderDBAsset configTarget = target as ShaderDBAsset;
                ShaderTool.AddAllMaterialsToHelper(configTarget);
            }
            EditorGUILayout.Space(10);

            base.OnInspectorGUI();
        }
    }
}
