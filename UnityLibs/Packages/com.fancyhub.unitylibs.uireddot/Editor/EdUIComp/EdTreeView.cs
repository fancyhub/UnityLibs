/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/18 15:04:20
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;


namespace FH.UI.Ed
{
    public interface IEdTreeViewItem<T>
    {
        string GetText(ValueTree<T> node);
        Texture2D GetIcon(ValueTree<T> node);
    }

    public class EdTreeView<T> : TreeView 
    {
        public InnerEdTreeViewItem _root;
        public Action<ValueTree<T>> EvtItemClick;
        public Action<ValueTree<T>> EvtItemDoubleClick;
        public IEdTreeViewItem<T> _data_getter;

        public class InnerEdTreeViewItem : TreeViewItem
        {
            public static int _id_gen = 0;
            public ValueTree<T> _node_val;
            public IEdTreeViewItem<T> _data_getter;

            public InnerEdTreeViewItem()
            {
                id = _id_gen++;
                children = new List<TreeViewItem>();
            }

            public void SetData(ValueTree<T> node_val, IEdTreeViewItem<T> data_getter)
            {
                children.Clear();
                _node_val = node_val;
                _data_getter = data_getter;
                if (_node_val == null)
                {
                    displayName = "";                    
                    return;
                }

                if (depth != -1)
                {
                    displayName = data_getter.GetText(node_val);
                    icon = data_getter.GetIcon(node_val);
                }

                foreach (var p in _node_val.GetChildren())
                {
                    InnerEdTreeViewItem sub_item = new InnerEdTreeViewItem();
                    sub_item.depth = depth + 1;
                    sub_item.SetData(p.Value, _data_getter);
                    AddChild(sub_item);
                }               
            }
        }

        public EdTreeView() : this(new TreeViewState())
        {

        }

        public EdTreeView(TreeViewState state) : base(state)
        {
            _root = new InnerEdTreeViewItem();
            _root.depth = -1;
        }

        public EdTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            _root = new InnerEdTreeViewItem();
            _root.depth = -1;
        }

        public void SelectData(T data)
        {
            SelectData(data, EqualityComparer<T>.Default);
        }

        public void SelectData(T data, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                return;

            var item = _find_item(ref data, comparer, _root);
            if (item == null)
                return;

            SetSelection(new List<int>() { item.id }, TreeViewSelectionOptions.None);
            _expend_item_parent(item);
        }

        public List<T> GetSelectData()
        {
            List<T> ret = new List<T>();
            foreach (var id in GetSelection())
            {
                InnerEdTreeViewItem item = FindItem(id, _root) as InnerEdTreeViewItem;
                if (item == null)
                    continue;
                if (item._node_val == null)
                    continue;
                ret.Add(item._node_val.Data);
            }
            return ret;
        }

        protected override void SingleClickedItem(int id)
        {
            InnerEdTreeViewItem item = FindItem(id, _root) as InnerEdTreeViewItem;
            if (item == null)
                return;
            _expend_item_parent(item);

            if (EvtItemClick == null)
                return;
            EvtItemClick.Invoke(item._node_val);
        }
        protected override void DoubleClickedItem(int id)
        {
            if (EvtItemDoubleClick == null)
                return;
            InnerEdTreeViewItem item = FindItem(id, _root) as InnerEdTreeViewItem;
            if (item == null)
                return;
            EvtItemDoubleClick.Invoke(item._node_val);
        }


        protected override void ContextClickedItem(int id)
        {
            InnerEdTreeViewItem item = FindItem(id, _root) as InnerEdTreeViewItem;
            if (item == null)
                return;
            var m = new GenericMenu();
            m.AddItem(new GUIContent("Hello"), false, _on_m_click, "Hello2");
            m.ShowAsContext();
        }

        public void _on_m_click(object user_data)
        {

        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            //return base.CanStartDrag(args);
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            base.SetupDragAndDrop(args);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return DragAndDropVisualMode.Move;
        }

        public void SetDataGetter(IEdTreeViewItem<T> data_getter)
        {
            _data_getter = data_getter;
        }

        public void SetData(ValueTree<T> data)
        {
            _root.SetData(data, _data_getter);
            Reload();
        }

        public void SetData(ValueTree<T> data, IEdTreeViewItem<T> data_getter)
        {
            _data_getter = data_getter;
            _root.SetData(data, data_getter);
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            return _root;
        }

        public void _expend_item_parent(TreeViewItem item)
        {
            for (TreeViewItem t = item; t != _root; t = t.parent)
            {
                SetExpanded(t.id, true);
            }
        }

        public InnerEdTreeViewItem _find_item(ref T data, IEqualityComparer<T> comparer, InnerEdTreeViewItem item)
        {
            foreach (var p in item.children)
            {
                var sub_item = p as InnerEdTreeViewItem;
                if (sub_item._node_val == null)
                    continue;
                if (comparer.Equals(sub_item._node_val.Data, data))
                {
                    return sub_item;
                }

                var result_item = _find_item(ref data, comparer, sub_item);
                if (result_item != null)
                    return result_item;
            }
            return null;
        }
    }

    //有search 
    public class EdTreeViewWithToolBar<T> : EdTreeView<T>
    {        
        public SearchField _search_field;
 
        public EdTreeViewWithToolBar()
        {
            _search_field = new SearchField();
        }
              

        public override void OnGUI(Rect rect)
        {
            Vector2 pos = rect.position;
            float search_height = 20;
            float collapse_btn_width = 80;
            float expand_btn_width = 80;
            float search_width_min = 99;

            float search_width = rect.width;
            if (search_width > (collapse_btn_width + search_width_min))
            {
                Rect rect_btn = new Rect();
                rect_btn.y = rect.y;
                rect_btn.x = rect.width - collapse_btn_width;
                rect_btn.width = collapse_btn_width;
                rect_btn.height = search_height;

                search_width -= collapse_btn_width;
                if (GUI.Button(rect_btn, "Collapse"))
                {
                    this.CollapseAll();
                }
            }

            if (search_width > (expand_btn_width + search_width_min))
            {
                Rect rect_btn = new Rect();
                rect_btn.y = rect.y;
                rect_btn.x = rect.width - expand_btn_width - collapse_btn_width;
                rect_btn.width = expand_btn_width;
                rect_btn.height = search_height;

                search_width -= expand_btn_width;
                if (GUI.Button(rect_btn, "Expand"))
                {
                    this.ExpandAll();
                }
            }

            Rect rect_search_field = new Rect();
            rect_search_field.position = pos;
            rect_search_field.width = search_width;
            rect_search_field.height = search_height;
            this.searchString = _search_field.OnToolbarGUI(rect_search_field, this.searchString);


            pos.y += search_height;
            Rect rect_tree_view = new Rect();
            rect_tree_view.position = pos;
            rect_tree_view.width = rect.width;
            rect_tree_view.height = rect.height - pos.y;

            base.OnGUI(rect_tree_view);
        }
    }
}
