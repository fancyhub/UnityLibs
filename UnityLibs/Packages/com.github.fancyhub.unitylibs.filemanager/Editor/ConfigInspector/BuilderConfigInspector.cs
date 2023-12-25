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

namespace FH.FileManagement.Ed
{

    [CustomEditor(typeof(FileBuilderConfig))]
    public class FileBuilderConfigInspector : Editor
    {

        private InnerSOListDrawer<BuildStep> _StepDrawer;        

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            FileBuilderConfig config = (FileBuilderConfig)target;            

            EditorGUILayout.Space(10);
            if (_StepDrawer == null)
            {
                _StepDrawer = new InnerSOListDrawer<BuildStep>(ref config.BuildSteps, AssetDatabase.GetAssetPath(target), "Add Step", (a) =>
                {
                    return a.Name;
                });
            }

            EditorGUILayout.Space(10);
            _StepDrawer.Draw("Build Steps");              
        }
    }
}
