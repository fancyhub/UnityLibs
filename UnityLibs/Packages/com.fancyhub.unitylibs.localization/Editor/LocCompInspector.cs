/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH
{
    public static class LocMgrEditorLoader
    { 
        public static void Init()
        {
            if (Application.isPlaying)
                return;

            if (LocMgr.FuncLoader != TableMgr.EdLoadTranslation)
            {
                LocMgr.FuncLoader = TableMgr.EdLoadTranslation;
                LocMgr.EdReloadAll();
            }
        }
    }

    [CustomEditor(typeof(LocComp), true)]
    public class LocCompInspector : Editor
    {
        private SerializedProperty _KeyProperty;
        private int _LangIndex;

        public virtual void OnEnable()
        {
            _KeyProperty = serializedObject.FindProperty("_LocKey");
            LocMgrEditorLoader.Init();
            ((LocComp)target).EdDoLocalize(LocLang.Lang);
            _LangIndex = LocLang.IndexOf(LocLang.Lang);
        }


        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();

            int index = EditorGUILayout.Popup("Lang", _LangIndex, LocLang.LangList);
            if (index != _LangIndex)
            {
                _LangIndex = index;
                string lang = LocLang.LangList[index];
                ((LocComp)target).EdDoLocalize(lang);
            }

            if (GUILayout.Button("Reload"))
            {
                LocMgr.EdReloadAll();
            }
            GUILayout.EndHorizontal();


            EditorGUILayout.PropertyField(_KeyProperty);
        }
    }
}
