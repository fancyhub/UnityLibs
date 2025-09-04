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
using FH.Ed;

namespace FH.Localization
{
    sealed class LocStrKeyBrowser : EditorWindow
    {
        internal static class Define
        {
            public const float WindowWidth = 800;
            public const float WindowHeight = 600;

            private const float TreeViewWidth = WindowWidth - 40;
            private const float TreeViewLeftPanding = (WindowWidth - TreeViewWidth) * 0.5f;

            public static readonly Rect TreeViewRect = new Rect(TreeViewLeftPanding, 50f, TreeViewWidth, WindowHeight - 90);
        }

        private SerializedProperty _keyProperty;
        private EdTableView<TableItem> _TreeView;

        internal class TableItem : IEdTableItem<TableItem>
        {
            public string Key;
            public string Translation;

            public int CompareTo(TableItem item, int field_index)
            {
                switch (field_index)
                {
                    case 0:
                        return Key.CompareTo(item.Key);
                    case 1:
                        return Translation.CompareTo(item.Translation);
                    default:
                        return 0;
                }
            }

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
            var stringKeyBrowser = LocStrKeyBrowser.CreateInstance<LocStrKeyBrowser>();
            stringKeyBrowser._keyProperty = prop;
            pos.position = GUIUtility.GUIToScreenPoint(pos.position);
            stringKeyBrowser.ShowAsDropDown(pos, new Vector2(Define.WindowWidth, Define.WindowHeight));
        }

        void OnGUI()
        {
            _Init();

            _TreeView.OnGUI(Define.TreeViewRect);

            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
            {
                Close();
                currentEvent.Use();
            }
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
                    //只取第一个
                    _Data.Add(new TableItem() { Key = p.Key, Translation = p.Value[0] });
                }
                _Data.Sort((a, b) => a.Key.CompareTo(b.Key));
            }

            if (_TreeView == null)
            {
                var list = LangSettingAsset.EdGetLangIdList();
                _TreeView = new EdTableView<TableItem>(new string[] { "Key", "Translation: " + list[0].Lang });

                string key_value = _keyProperty.stringValue;
                var selected_index = _Data.FindIndex(element => { return element.Key == key_value; });
                _TreeView.SetData(_Data, selected_index);
                _TreeView.SelectionChangedCallback += _OnSelectionChanged;
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
        }
    }
}