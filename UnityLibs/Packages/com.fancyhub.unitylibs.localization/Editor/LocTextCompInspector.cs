/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace FH
{
    [CustomEditor(typeof(FH.UI.LocTextComp), true)]
    public class LocTextCompInspector : LocCompInspector
    {
        private SerializedProperty _StyleProperty;
        private SerializedProperty _ArgsProperty;
        private SerializedProperty _KeyProperty;
        public override void OnEnable()
        {
            base.OnEnable();
            _StyleProperty = serializedObject.FindProperty("_Style");
            _ArgsProperty = serializedObject.FindProperty("_Arguments");
            _KeyProperty = serializedObject.FindProperty("_LocKey.Key");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(_StyleProperty);
            serializedObject.ApplyModifiedProperties();
            _DrawArgs();
        }

        private void _DrawArgs()
        {
            if (string.IsNullOrEmpty(_KeyProperty.stringValue))
            {
                _ArgsProperty.arraySize = 0;
                return;
            }

            if (!LocMgr.EdTryGetTrans(_KeyProperty.stringValue, out var trans) || trans == null || trans.Length == 0)
            {
                _ArgsProperty.arraySize = 0;
                return;
            }

            string txt = trans[0];
            if (string.IsNullOrEmpty(txt))
            {
                _ArgsProperty.arraySize = 0;
                return;
            }

            IEnumerable<(int index, string format)> placeholders = FormatSnooper.GetFormatPlaceholders(txt);
            List<int> lstPlaceholderIndex = new List<int>();
            foreach (var ph in placeholders)
            {
                if (false == lstPlaceholderIndex.Contains(ph.index))
                    lstPlaceholderIndex.Add(ph.index);
            }
            if (lstPlaceholderIndex.Count == 0)
            {
                _ArgsProperty.arraySize = 0;
                return;
            }
            lstPlaceholderIndex.Sort();

            _ArgsProperty.arraySize = lstPlaceholderIndex[lstPlaceholderIndex.Count - 1] + 1;

            EditorGUILayout.LabelField("Arguments");
            EditorGUI.indentLevel++;
            for (int index = 0; index < _ArgsProperty.arraySize; ++index)
            {
                var argument = _ArgsProperty.GetArrayElementAtIndex(index);
                var formatIndex = lstPlaceholderIndex[index];
                EditorGUILayout.PropertyField(argument, new GUIContent("{" + formatIndex.ToString() + "}"));
            }
            EditorGUI.indentLevel--;
        }


        private class FormatSnooper : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type formatType)
            {
                return this;
            }

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                Placeholders.Add(((int)arg, format));
                return null;
            }

            internal readonly List<(int index, string format)> Placeholders = new List<(int index, string format)>();

            public static IEnumerable<(int index, string format)> GetFormatPlaceholders(string format, int max_count = 100)
            {
                var snooper = new FormatSnooper();
                string.Format(
                    snooper,
                    format,
                    Enumerable.Range(0, max_count).Cast<object>().ToArray()
                );

                return snooper.Placeholders;
            }
        }
    }
}
