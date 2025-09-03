using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Globalization;

namespace FH
{
    class LangBrowser : EditorWindow
    {

        internal static class Define
        {
            public const float WindowWidth = 400;
            public const float WindowHeight = 600;

            private const float TreeViewWidth = WindowWidth - 40;
            private const float TreeViewLeftPanding = (WindowWidth - TreeViewWidth) * 0.5f;

            public static readonly Rect ToolbarRect = new Rect(TreeViewLeftPanding, 30f, TreeViewWidth, 20f);
            public static readonly Rect TreeViewRect = new Rect(TreeViewLeftPanding, 50f, TreeViewWidth, WindowHeight - 110);

            public const float ColumnEnableWidth = 50;
            public const float ColumnLangWidth = TreeViewWidth - ColumnEnableWidth;

            public const float WindowFooterHeight = 150;

            public static GUIContent ButtonApply = new GUIContent("Apply");
            public static GUIContent LableSource = new GUIContent("Locale Source", "Source data for generating the locales");
            public const string progressTitle = "Generating Locales";
            public const string saveDialog = "Save locales to folder";

            public static GUIContent[] FooterButtonnames =
            {
                new GUIContent("Select All", "Select all visible locales"),
                new GUIContent("Deselect All", "Deselect all visible locales")
            };
        }


        private class Item : IEdTableItem
        {
            public string Lang;
            public bool Enable;
            public Item(string Lang, bool Enable)
            {
                this.Lang = Lang;
                this.Enable = Enable;
            }
            public bool IsMatch(string search)
            {
                if (Lang.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                return false;
            }

            public void OnDrawField(Rect cellRect, int field_index)
            {
                switch (field_index)
                {
                    case 0:
                        Enable = EditorGUI.Toggle(cellRect, Enable);
                        break;

                    case 1:
                        EditorGUI.LabelField(cellRect, Lang);
                        break;

                }
            }
        }

        private Dictionary<string, Item> _AllItems = new();
        private List<Item> _CultureInfoList = new List<Item>();
        private List<Item> _SystemLangList = new List<Item>();

        private enum LocaleSource
        {
            CultureInfo,
            SystemLanguage
        }
        private LocaleSource _LocaleSource;
        private SearchField _SearchField;
        private EdTableView<Item> _ListView;
        private static LangSettingAsset _LangSettingAsset;

        public static void ShowWindow(LangSettingAsset asset)
        {
            _LangSettingAsset = asset;

            var window = (LangBrowser)GetWindow(typeof(LangBrowser));
            window.titleContent = new GUIContent("Locale Generator");
            window.minSize = new Vector2(500, 500);
            window.Show();
        }

        void OnEnable()
        {
            _AllItems.Clear();
            {
                _CultureInfoList.Clear();
                foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    if (cultureInfo.LCID == CultureInfo.InvariantCulture.LCID)
                        continue;
                    // Ignore legacy cultures
                    if (cultureInfo.EnglishName.Contains("Legacy"))
                        continue;


                    if (!_AllItems.TryGetValue(cultureInfo.Name, out var item))
                    {
                        bool exist = _LangSettingAsset.Setting.IndexOfLang(cultureInfo.Name) >= 0;
                        item = new Item(cultureInfo.Name, exist);
                        _AllItems[cultureInfo.Name] = item;
                    }
                    _CultureInfoList.Add(item);
                }
            }

            {
                _SystemLangList.Clear();
                for (int i = 0; i < (int)SystemLanguage.Unknown; ++i)
                {
                    string lang = LangSetting.GetFromSystemLang((SystemLanguage)i);
                    if (string.IsNullOrEmpty(lang))
                        continue;

                    if (!_AllItems.TryGetValue(lang, out var item))
                    {
                        bool exist = _LangSettingAsset.Setting.IndexOfLang(lang) >= 0;
                        item = new Item(lang, exist);
                        _AllItems[lang] = item;
                    }
                    _SystemLangList.Add(item);
                }
            }

            _ListView = new EdTableView<Item>(_CreateHeader());
            _ListView.SetData(_CultureInfoList, -1);
            _SearchField = new SearchField();
            _SearchField.downOrUpArrowKeyPressed += _ListView.SetFocusAndEnsureSelectedItem;
        }

        private static MultiColumnHeader _CreateHeader()
        {
            var list = LangSettingAsset.EdGetLangIdList();
            string first_name = list[0].Lang;
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(" "),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = Define.ColumnEnableWidth,
                    minWidth = 10,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Lang"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width =  Define.ColumnLangWidth,
                    minWidth = 10,
                    autoResize = true,
                    allowToggleVisibility = false
                }
            };
            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        void OnGUI()
        {
            var newSource = (LocaleSource)EditorGUILayout.EnumPopup(Define.LableSource, _LocaleSource);
            var list_item = _GetCurrentList();
            if (_LocaleSource != newSource)
            {
                _LocaleSource = newSource;
                list_item = _GetCurrentList();
                _ListView.SetData(list_item, -1);
            }

            _ListView.searchString = _SearchField.OnToolbarGUI(_ListView.searchString);
            var rect = EditorGUILayout.GetControlRect(false, position.height - Define.WindowFooterHeight);
            _ListView.OnGUI(rect);

            var selection = GUILayout.Toolbar(-1, Define.FooterButtonnames, EditorStyles.toolbarButton);
            if (selection == 0)
            {
                foreach (var p in list_item)
                {
                    p.Enable = true;
                }
            }
            else if (selection == 1)
            {
                foreach (var p in list_item)
                {
                    p.Enable = false;
                }
            }


            using (new EditorGUI.DisabledScope(_IsAllEmpty()))
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Define.ButtonApply, GUILayout.Width(180)))
                {
                    List<string> langs = new List<string>();
                    foreach (var p in _AllItems)
                    {
                        if (p.Value.Enable)
                            langs.Add(p.Key);
                    }

                    if (langs.Count > 0)
                    {
                        _LangSettingAsset.Setting.EdChangeList(langs);
                        UnityEditor.EditorUtility.SetDirty(_LangSettingAsset);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private bool _IsAllEmpty()
        {
            if (_AllItems.Count == 0)
                return true;
            foreach (var p in _AllItems)
            {
                if (p.Value.Enable)
                    return false;
            }
            return true;
        }

        private List<Item> _GetCurrentList()
        {
            switch (_LocaleSource)
            {
                case LocaleSource.CultureInfo:
                    return _CultureInfoList;

                default: return _SystemLangList;

            }
        }
    }
}