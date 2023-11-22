using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.UI.View.Gen.ED
{
    [CustomEditor(typeof(UIViewGenConfig))]
    public class UIViewGenConfigInsepector : Editor
    {
        public override void OnInspectorGUI()
        {
            var config = (UIViewGenConfig)target;

            if (config.EdIsCurrentDefault())
            {
                GUILayout.Label("Self Is Default: " + config.EdGetSelfPath());
            }
            else if(GUILayout.Button("Set Default"))
            {
                config.EdSetCurrentDefault();
            }            

            GUILayout.Space(10);
            base.OnInspectorGUI();
        }
    }
}
