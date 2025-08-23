/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;


namespace FH
{
    [CustomPropertyDrawer(typeof(LocKey))]
    public class LocStrKeyDrawer : PropertyDrawer
    {
        private static Texture _SearchIcon;
        private static GUIStyle _SearchBtnStyle;
        private static GUIStyle _TranslationStyleOK;
        private static GUIStyle _TranslationStyleError;

        private static void _Init()
        {
            if (_SearchIcon == null)
                _SearchIcon = UnityEditor.EditorGUIUtility.FindTexture("Search Icon");

            if (_SearchBtnStyle == null)
            {
                _SearchBtnStyle = new GUIStyle(GUI.skin.button);
                _SearchBtnStyle.padding.top = 1;
                _SearchBtnStyle.padding.bottom = 1;
            }

            if (_TranslationStyleOK == null)
            {
                _TranslationStyleOK = new GUIStyle(EditorStyles.textField);
                _TranslationStyleOK.normal.textColor = Color.white;
            }

            if (_TranslationStyleError == null)
            {
                _TranslationStyleError = new GUIStyle(EditorStyles.textField);
                _TranslationStyleError.normal.textColor = Color.red;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            string key = property.FindPropertyRelative("Key").stringValue;
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (string.IsNullOrEmpty(key) || !LocMgr.EdContainsKey(key))
            {
                height += EditorGUIUtility.singleLineHeight;
                return height;
            }
            height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * LocLang.LangList.Length;
            return height;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _Init();
            Rect rawPosition = position;
            if (LocMgr.EdKeyCount == 0)
            {
                EditorGUI.HelpBox(position, "Please Import Translations First.", MessageType.Warning);
                return;
            }

            SerializedProperty keyProperty = property.FindPropertyRelative("Key");
            string key = keyProperty.stringValue;

            {
                EditorGUI.BeginProperty(position, label, property);
                float baseHeight = GUI.skin.textField.CalcSize(label).y;
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                Rect searchRect = new Rect(position.x + position.width - _SearchIcon.width - 7, position.y, _SearchIcon.width + 7, baseHeight);
                Rect idRect = new Rect(position.x, position.y, searchRect.x - position.x - 3, baseHeight);
                EditorGUI.PropertyField(idRect, keyProperty, GUIContent.none);

                if (GUI.Button(searchRect, new GUIContent(_SearchIcon, "Search"), _SearchBtnStyle))
                {
                    LocStrKeyBrowser.Browser.Show(keyProperty, position);
                }
                EditorGUI.EndProperty();
            }

            {
                if (string.IsNullOrEmpty(key))
                {
                    var helpPosition = rawPosition;
                    helpPosition.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                    helpPosition.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.HelpBox(helpPosition, "Please select StringKey", MessageType.Info);
                    return;
                }

                if (!LocMgr.EdTryGetTrans(key, out var all_trans))
                {
                    var helpPosition = rawPosition;
                    helpPosition.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                    helpPosition.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.HelpBox(helpPosition, "Invalid StringKey", MessageType.Error);
                    return;
                }

                Rect translationPosition = rawPosition;
                translationPosition.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                translationPosition.height = EditorGUIUtility.singleLineHeight;

                string[] lang_list = LocLang.LangList;

                EditorGUI.indentLevel++;
                for (int i = 0; i < lang_list.Length; i++)
                {
                    string code = lang_list[i];
                    string tran = all_trans[i];

                    if (!string.IsNullOrEmpty(tran))
                        EditorGUI.LabelField(translationPosition, code, tran, _TranslationStyleOK);
                    else
                        EditorGUI.LabelField(translationPosition, code, "<Emtpy Translation>", _TranslationStyleError);

                    translationPosition.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
