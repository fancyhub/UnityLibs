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
using UnityEditor.IMGUI.Controls;

namespace FH.Ed
{
    public interface IEdTableViewItem<T>
    {
        bool IsMatch(string search);
        void OnDrawField(Rect cellRect, int field_index);

        int CompareTo(T item, int field_index);
    }

    public class EdTableView<T> : TreeView where T : class, IEdTableViewItem<T>
    {
        private const float RowHeight = 20;
        private const float kToggleWidth = 18f;
        private readonly static float SearchFieldHeight = EditorGUIUtility.singleLineHeight;

        public Action<int, T> SelectionChangedCallback { get; set; }

        sealed private class InnerTableViewItem : TreeViewItem
        {
            public readonly T Data;
            public InnerTableViewItem(int index, T data) : base(index, 0) { this.Data = data; }
            public bool IsMatch(string search)
            {
                return Data.IsMatch(search);
            }
        }

        private TreeViewItem _Root;
        private List<InnerTableViewItem> _AllItemList = new List<InnerTableViewItem>();
        private List<TreeViewItem> _TempFilterItemViewList = new List<TreeViewItem>();
        private bool _ResizeColWidth = false;
        private SearchField _SearchField;

        public SearchField SearchField => _SearchField;
        public bool EnableSearchField { get; set; } = true;


        public EdTableView(MultiColumnHeader multiColumnHeader) : base(new TreeViewState(), multiColumnHeader)
        {
            rowHeight = RowHeight;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (RowHeight - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kToggleWidth;

            _SearchField = new SearchField();
            _SearchField.downOrUpArrowKeyPressed += this.SetFocusAndEnsureSelectedItem;

            this.multiColumnHeader.sortingChanged += _OnSortingChanged;
        }



        public EdTableView(string[] headerNames) : this(_CreateHeader(headerNames))
        {
            _ResizeColWidth = true;
        }


        public void OnGUI(float width, float height)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.width = width;
            OnGUI(rect);
        }

        public override void OnGUI(Rect rect)
        {
            if (_ResizeColWidth)
            {
                _ResizeColWidth = false;
                int count = this.multiColumnHeader.state.columns.Length;
                float colWidth = (rect.width-20) / count;
                foreach (var p in this.multiColumnHeader.state.columns)
                    p.width = colWidth;
            }

            if (EnableSearchField)
            {
                Rect searchFieldRect = new Rect(rect.x, rect.y, rect.width, SearchFieldHeight);
                searchString = _SearchField.OnToolbarGUI(searchFieldRect, searchString);

                rect.height -= searchFieldRect.height;
                rect.y += searchFieldRect.height;
            }

            base.OnGUI(rect);
        }

        public void SetData(List<T> data, int selected_index = -1)
        {
            _AllItemList.Clear();
            foreach (var p in data)
            {
                _AllItemList.Add(new InnerTableViewItem(_AllItemList.Count, p));
            }

            Reload();

            if (selected_index >= 0)
            {
                SetSelection(new List<int>() { selected_index });
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            _TempFilterItemViewList.Clear();
            if (_Root == null)
            {
                _Root = new TreeViewItem(-1, -1);
            }

            if (_Root.children == null)
                _Root.children = new List<TreeViewItem>();
            _Root.children.Clear();
            _Root.children.AddRange(_AllItemList);
            return _Root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (root.depth != -1)
                return base.BuildRows(root);

            _TempFilterItemViewList.Clear();
            if (string.IsNullOrEmpty(searchString))
            {
                _TempFilterItemViewList.AddRange(_AllItemList);
                return _TempFilterItemViewList;
            }

            foreach (var p in _AllItemList)
            {
                if (p.IsMatch(searchString))
                {
                    _TempFilterItemViewList.Add(p);
                }
            }
            return _TempFilterItemViewList;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (InnerTableViewItem)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                Rect cellRect = args.GetCellRect(i);

                //CenterRectUsingSingleLineHeight(ref cellRect);
                //var c=  this.multiColumnHeader.GetColumn(args.GetColumn(i));

                item.Data.OnDrawField(cellRect, i);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            if (SelectionChangedCallback != null)
            {
                int index = selectedIds[0];
                SelectionChangedCallback(index, _AllItemList[index].Data);
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item) { return false; }

        private void _OnSortingChanged(MultiColumnHeader header)
        {
            int index = header.sortedColumnIndex;
            var column = header.GetColumn(index);
            int neg = column.sortedAscending ? -1 : 1;


            _AllItemList.Sort((a, b) =>
            {
                return neg * a.Data.CompareTo(b.Data, index);
            });

            Reload();
        }

        private static MultiColumnHeader _CreateHeader(string[] headerNames)
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[headerNames.Length];
            for (int i = 0; i < headerNames.Length; i++)
            {
                columns[i] = new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent(headerNames[i]),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    minWidth = 10,
                    autoResize = true,
                    allowToggleVisibility = false
                };
            }
            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

    }
}
