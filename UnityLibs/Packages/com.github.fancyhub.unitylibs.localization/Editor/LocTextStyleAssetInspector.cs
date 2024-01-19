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
    [CustomEditor(typeof(LocTextStyleAsset))]
    public class LocTextStyleAssetInspector : Editor
    {
        private static string[] _HeaderList = new string[] { "Lang", "Font", "FontStyle", "FontSize", "LineSpace" };
        private static string[] _NameList = new string[] { "Lang", "Font", "FontStyle", "FontSize", "LineSpace" };

        private ReorderableList _List;

        private void OnEnable()
        {
            _List = new ReorderableList(serializedObject, serializedObject.FindProperty("StyleList"), false, true, false, false);
            _List.drawElementCallback = _DrawItem;
            _List.drawHeaderCallback = _DrawHeader;

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Add All"))
            {
                ((LocTextStyleAsset)target).EdCreateAll();
            }
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
