using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.UI.ViewGenerate.Ed
{
    [CustomEditor(typeof(UIViewGeneratorConfig))]
    public class UIViewGenConfigInsepector : Editor
    {
        public override void OnInspectorGUI()
        {
            var config = (UIViewGeneratorConfig)target;

            if (config.IsCurrentDefault())
            {
                GUILayout.Label("Self Is Default: " + config.GetSelfPath());
            }
            else if(GUILayout.Button("Set Default"))
            {
                config.SetCurrentDefault();
            }            

            GUILayout.Space(10);
            base.OnInspectorGUI();
        }
    }
}
