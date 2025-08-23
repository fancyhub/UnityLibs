/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace FH.LocStrKeyBrowser
{
    internal static class BrowserDefine
    {
        public const float WindowWidth = 800;
        public const float WindowHeight = 600;

        private const float TreeViewWidth = WindowWidth - 40;
        private const float TreeViewLeftPanding = (WindowWidth - TreeViewWidth) * 0.5f;

        public static readonly Rect ToolbarRect = new Rect(TreeViewLeftPanding, 30f, TreeViewWidth, 20f);
        public static readonly Rect TreeViewRect = new Rect(TreeViewLeftPanding, 50f, TreeViewWidth, WindowHeight - 110);

        public const float ColumnKeyWidth = TreeViewWidth * 0.5f - 5;
        public const float ColumnTranWidth = TreeViewWidth * 0.5f;
    }


    public sealed class Browser : EditorWindow
    {
        private SerializedProperty _keyProperty;
        private SearchField _SearchField;
        private EdTableView<TableItem> _TreeView;

        internal class TableItem : IEdTableItem
        {
            public string Key;
            public string Translation;

            public bool IsMatch(string search)
            {
                if (Key.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                if (Translation.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                return false;
            }

            public void OnDrawField(Rect cellRect, int field_index)
            {
                switch (field_index)
                {
                    case 0:
                        EditorGUI.LabelField(cellRect, Key);
                        break;

                    case 1:
                        EditorGUI.LabelField(cellRect, Translation);
                        break;

                }
            }
        }

        private List<TableItem> _Data;

        public static void Show(SerializedProperty prop, Rect pos)
        {
            var stringKeyBrowser = Browser.CreateInstance<Browser>();
            stringKeyBrowser._keyProperty = prop;
            pos.position = GUIUtility.GUIToScreenPoint(pos.position);
            stringKeyBrowser.ShowAsDropDown(pos, new Vector2(BrowserDefine.WindowWidth, BrowserDefine.WindowHeight));
        }

        void OnGUI()
        {
            _Init();

            string search_string = _SearchField.OnGUI(BrowserDefine.ToolbarRect, _TreeView.searchString);
            if (search_string != _TreeView.searchString)
                _TreeView.searchString = search_string;
            _TreeView.OnGUI(BrowserDefine.TreeViewRect);
        }

        private void OnDestroy()
        {
            _keyProperty = null;
        }

        private void _Init()
        {
            if (_Data == null)
            {
                _Data = new List<TableItem>(LocMgr.EdAllData.Count);
                foreach (var p in LocMgr.EdAllData)
                {
                    //只取第一个Trans
                    _Data.Add(new TableItem() { Key = p.Key, Translation = p.Value[0] });
                }
                _Data.Sort((a, b) => a.Key.CompareTo(b.Key));
            }

            if (_TreeView == null)
            {
                _TreeView = new EdTableView<TableItem>(_CreateHeader());

                string key_value = _keyProperty.stringValue;
                var selected_index = _Data.FindIndex(element => { return element.Key == key_value; });
                _TreeView.SetData(_Data, selected_index);
                _TreeView.ExpandAll();


                _TreeView.SelectionChangedCallback += _OnSelectionChanged;
            }

            if (_SearchField == null)
            {
                _SearchField = new SearchField();
                _SearchField.downOrUpArrowKeyPressed += _TreeView.SetFocusAndEnsureSelectedItem;
            }
        }

        private void _OnSelectionChanged(int selectedId, TableItem item)
        {
            if (null != _keyProperty && _keyProperty.serializedObject != null)
            {
                _keyProperty.serializedObject.Update();
                _keyProperty.stringValue = item.Key;
                _keyProperty.serializedObject.ApplyModifiedProperties();

                if (_keyProperty.serializedObject.targetObject is LocComp loc_comp)
                {
                    if (Application.isPlaying)
                    {
                        loc_comp.DoLocalize();
                    }
                }
            }
            Close();
        }

        private static MultiColumnHeader _CreateHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Key"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = BrowserDefine.ColumnKeyWidth,
                    minWidth = 10,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Translation: "+LocLang.LangList[0]),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width =  BrowserDefine.ColumnTranWidth,
                    minWidth = 10,
                    autoResize = true,
                    allowToggleVisibility = false
                }
            };
            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }
    }
}
#endif