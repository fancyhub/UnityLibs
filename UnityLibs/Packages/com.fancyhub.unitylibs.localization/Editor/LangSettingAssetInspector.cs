/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace FH
{
    [CustomEditor(typeof(FH.LangSettingAsset))]
    public class LangSettingAssetInsepector : Editor
    {
        private static string[] _HeaderList = new string[] { "Lang", "Enable" };
        private static string[] _NameList = new string[] { "Lang", "Enable" };

        private ReorderableList _List;

        private void OnEnable()
        {
            _List = new ReorderableList(serializedObject, serializedObject.FindProperty("Setting.Langs"), true, true, true, true);
            _List.drawElementCallback = _DrawItem;
            _List.drawHeaderCallback = _DrawHeader;
            _List.onAddCallback = _AddItem;

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                var setting = ((LangSettingAsset)target).Setting;
                int index = setting.IndexOfLang(setting.FallbackLang);
                List<string> list = new List<string>();
                foreach (var item in setting.Langs)
                {
                    if (item.Enable)
                        list.Add(item.Lang);
                }
                int newIndex = EditorGUILayout.Popup("Fallback",index, list.ToArray());
                if (newIndex != index)
                {
                    if (newIndex < 0)
                    {
                        setting.FallbackLang = "";
                    }
                    else
                    {
                        setting.FallbackLang = list[newIndex];
                    }
                }
            }
            _List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

        }

        private void _AddItem(ReorderableList list)
        {

            LangBrowser.ShowWindow((LangSettingAsset)target);
        }

        private void _DrawHeader(Rect rect)
        {
            float sub_width = rect.width / _NameList.Length;
            for (int i = 0; i < _HeaderList.Length; i++)
            {
                Rect sub_rect = new Rect(rect.x + sub_width * i, rect.y, sub_width, rect.height);
                EditorGUI.LabelField(sub_rect, _HeaderList[i]);
            }
        }

        private void _DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty item = _List.serializedProperty.GetArrayElementAtIndex(index);
            rect.height = EditorGUIUtility.singleLineHeight;

            float sub_width = rect.width / _NameList.Length;
            for (int i = 0; i < _NameList.Length; i++)
            {
                Rect sub_rect = new Rect(rect.x + sub_width * i, rect.y, sub_width, rect.height);
                SerializedProperty sub_item = item.FindPropertyRelative(_NameList[i]);
                if (i == 0)
                    EditorGUI.LabelField(sub_rect, sub_item.stringValue, EditorStyles.label);
                else
                    EditorGUI.PropertyField(sub_rect, sub_item, GUIContent.none);
            }
        }
    }
}
