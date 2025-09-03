/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using FH.UI;
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
            ((LocComp)target).EdDoLocalize(LocMgr.CurrentLang);
            _LangIndex = LangSettingAsset.EdIndexOfLang(LocMgr.CurrentLang);
        }


        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();

            string[] lang_list = LangSettingAsset.EdGetLangNameList();
            int index = EditorGUILayout.Popup("Lang", _LangIndex, lang_list);
            if (index != _LangIndex)
            {
                _LangIndex = index;
                string lang = lang_list[index];
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
