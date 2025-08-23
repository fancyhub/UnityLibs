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
    public interface IEdTableItem
    {
        bool IsMatch(string search);
        void OnDrawField(Rect cellRect, int field_index);
    }

    public class EdTableView<T> : TreeView where T : IEdTableItem
    {
        private const float RowHeight = 20;
        private const float kToggleWidth = 18f;

        public Action<int, T> SelectionChangedCallback { get; set; }


        sealed private class TableViewItem : TreeViewItem
        {
            public readonly T Data;
            public TableViewItem(int index, T data) : base(index, 0) { this.Data = data; }
            public bool IsMatch(string search)
            {
                return Data.IsMatch(search);
            }
        }

        private TreeViewItem _Root;
        private List<TableViewItem> _AllItemList = new List<TableViewItem>();
        private List<TreeViewItem> _TempFilterItemViewList = new List<TreeViewItem>();

        public EdTableView(MultiColumnHeader multiColumnHeader) : base(new TreeViewState(), multiColumnHeader)
        {
            rowHeight = RowHeight;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (RowHeight - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kToggleWidth;
        }

        public void SetData(List<T> data, int selected_index)
        {
            _AllItemList.Clear();
            foreach (var p in data)
            {
                _AllItemList.Add(new TableViewItem(_AllItemList.Count, p));
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
            var item = (TableViewItem)args.item;
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                Rect cellRect = args.GetCellRect(i);
                CenterRectUsingSingleLineHeight(ref cellRect);
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
    }
}
#endif