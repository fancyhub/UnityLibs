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
using UnityEditorInternal;
using FH.Ed;

namespace FH.AssetBundleBuilder.Ed
{
    [CustomEditor(typeof(AssetsInputConfig))]
    public class AssetsInputConfigInspector : UnityEditor.Editor
    {
        private static string[] _HeaderList = new string[] { "Asset(Drag)", "AddressMode", "BundleName", "Tags(eg: tag_a;tag_b)" };
        private static string[] _NameList = new string[] { "Asset", "AddressMode", "BundleName", "Tags" };

        private ReorderableList _List;

        private void OnEnable()
        {
            _List = new ReorderableList(serializedObject, serializedObject.FindProperty("Items"), true, true, true, true);
            _List.drawElementCallback = _DrawItem;
            _List.drawHeaderCallback = _DrawHeader;

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
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
            //[Header("eg:base;tag_a;tab_b")]
            SerializedProperty item = _List.serializedProperty.GetArrayElementAtIndex(index);
            rect.height = EditorGUIUtility.singleLineHeight;

            float sub_width = rect.width / _NameList.Length;
            for (int i = 0; i < _NameList.Length; i++)
            {
                Rect sub_rect = new Rect(rect.x + sub_width * i, rect.y, sub_width, rect.height);
                SerializedProperty sub_item = item.FindPropertyRelative(_NameList[i]);                
                EditorGUI.PropertyField(sub_rect, sub_item, GUIContent.none);
            }
        }
    }
}
