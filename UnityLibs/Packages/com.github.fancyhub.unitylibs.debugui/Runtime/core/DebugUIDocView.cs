/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FH.DebugUI
{
    public class DebugUIDocView : VisualElement
    {
        public const string StyleNameRoot = "debug_ui_root";
        public const string StyleNameTreeContainer = "debug_ui_tree_container";
        public const string StyleNameTree = "debug_ui_tree";
        public const string StyleNameTreeItem = "debug_ui_tree_item";
        public const string StyleNameContentContainer = "debug_ui_content_container";

        private VisualElement _TreeContainer;
        private VisualElement _ContentContainer;
        private TreeView _TreeView;
        private Button _CloseButton;

        public Action CloseAction;

        public VisualElement TreeContainer => _TreeContainer;
        public VisualElement ContentContainer => _ContentContainer;

        public DebugUIDocView()
        {
            // 创建主容器（水平布局）            
            this.style.flexDirection = FlexDirection.Row;
            this.style.flexGrow = 1;
            this.name = StyleNameRoot;

            // 左侧树状图容器
            _TreeContainer = new VisualElement();           
            _TreeContainer.name = StyleNameTreeContainer;
            Add(_TreeContainer);


            // 创建树状图
            _TreeView = new TreeView();
            _TreeView.name = StyleNameTree;
            {
                _TreeView.makeItem = () =>
                {
                    var ret = new Label();
                    ret.style.alignSelf = Align.FlexStart;
                    ret.style.unityTextAlign = TextAnchor.MiddleCenter;
                    ret.name = StyleNameTreeItem;
                    return ret;
                };
                _TreeView.bindItem = (element, index) =>
                {
                    var item = _TreeView.GetItemDataForIndex<DebugUIItem>(index);
                    (element as Label).text = item.Name;
                };
                _TreeView.selectionChanged += _OnTreeSelected;
            }
            _TreeView.style.flexGrow = 1;
            _TreeContainer.Add(_TreeView);


            // 右侧内容容器
            ScrollView contentScroll = new ScrollView();
            contentScroll.name = StyleNameContentContainer;
            contentScroll.style.flexGrow = 1;
            Add(contentScroll);
            _ContentContainer = contentScroll;


            _CloseButton = new Button(_OnCloseBtnClick);
            _CloseButton.text = "X";
            _CloseButton.style.position = Position.Absolute;
            _CloseButton.style.right = 0;
            Add(_CloseButton);
        }

        internal void SetTreeViewData(DebugUIItem data)
        {
            List<TreeViewItemData<DebugUIItem>> tree_view_items = new List<TreeViewItemData<DebugUIItem>>();
            if (data != null && data.Children != null)
            {
                foreach (var p in data.Children)
                {
                    tree_view_items.Add(_CreateTreeViewItem(p));
                }
            }

            _TreeView.SetRootItems(tree_view_items);

            _TreeView.selectedIndex = 0;
        }

        private void _OnCloseBtnClick()
        {
            CloseAction?.Invoke();
        }

        private void _OnTreeSelected(IEnumerable<object> source)
        {
            DebugUIItem selected_item = null;
            foreach (var p in source)
            {
                selected_item = (DebugUIItem)p;
                break;
            }

            if (selected_item == null || selected_item.Views == null)
                return;

            _ContentContainer.Clear();
            foreach (var view in selected_item.Views)
                view.OnDebugUIItemEnable(_ContentContainer);
        }

        private static TreeViewItemData<DebugUIItem> _CreateTreeViewItem(DebugUIItem item)
        {
            if (item.Children == null)
                return new TreeViewItemData<DebugUIItem>(item.Id, item);

            var child_list = new List<TreeViewItemData<DebugUIItem>>();
            foreach (var p in item.Children)
                child_list.Add(_CreateTreeViewItem(p));

            return new TreeViewItemData<DebugUIItem>(item.Id, item, child_list);
        }
    }
}
