/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/31 11:39:07
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public interface IUIScrollListItemFactory<TData>
    {
        /// <summary>
        /// 根据 数据类型来创建 item
        /// </summary>        
        IScrollListItem<TData> CreateScrollItem(TData data);

        //重用的时候，需要判断该对象类型，能否使用该 数据
        bool IsScrollItemSuite(IScrollListItem<TData> item, TData data);

        void CheckViewClickable();
    }

    public delegate void ScrollItemClickCB(int index, long user_data);

    public interface IUIScrollBinder
    {
        void SetData(IList data);
        IList GetData();
        void SetItemClickCB(ScrollItemClickCB cb);
        void ClearItems();
        void Select(int index);
        void Destroy();
    }

    public interface IUIScrollBinder<TData> : IUIScrollBinder
    {
        void SetTData(IList<TData> data);
        IList<TData> GetTData();
        void SetCompCreateCB(Action<TData, IUIView> cb);
    }

    /// <summary>
    /// 一个数据绑定类
    /// </summary>
    public class FUIScrollBinder<TData> : IUIScrollBinder<TData>
    {
        public static TData[] _empty = new TData[0];
        public static LinkedList<IScrollListItem<TData>> _item_cache_list = new LinkedList<IScrollListItem<TData>>();

        public FUIScroll _scroller;
        public IUIScrollListItemFactory<TData> _item_factory;
        public List<IScrollListItem<TData>> _item_list = new List<IScrollListItem<TData>>();
        public IList<TData> _data;
        public ScrollItemClickCB _item_click_cb;
        public Action<TData, IUIView> _view_create_cb;

        public FUIScrollBinder(FUIScroll scroller, IUIScrollListItemFactory<TData> item_factory)
        {
            _scroller = scroller;
            _item_factory = item_factory;
        }

        public IList GetData() { return _data as IList; }
        public IList<TData> GetTData() { return _data; }

        public virtual void SetData(IList data)
        {
            IList<TData> t_data = data as IList<TData>;
            Log.Assert(!(data != null && t_data == null), "Scroll Binder 传入的数据类型不对");

            SetTData(t_data);
        }


        public virtual void SetTData(IList<TData> data)
        {
            _data = data;
            //保证_data变量永远不为空，避免在单个添加的时候报空
            if (_data == null) _data = _empty;

            if (_item_list.Capacity < _data.Count)
                _item_list.Capacity = _data.Count;

            _scroller.BeginBatch();

            int min_count = Mathf.Min(_item_list.Count, _data.Count);

            int min_same_count = min_count;
            //重新同步相同的
            for (int i = 0; i < min_count; ++i)
            {
                if (_item_factory.IsScrollItemSuite(_item_list[i], _data[i]))
                {
                    _item_list[i].SetData(_data[i]);
                    continue;
                }
                min_same_count = i;
            }

            //如果有类型不一致的
            if (min_same_count < min_count)
            {
                _item_cache_list.Clear();
                //1. 把从不相同的item开始, 剩下的全部先移动到 cache list里面
                for (int i = _item_list.Count - 1; i > min_same_count; i--)
                {
                    var item = _item_list[i];
                    _scroller.RemoveItem(item, false);
                    _item_cache_list.ExtAddFirst(item);
                    _item_list.RemoveAt(i);
                }

                //2. 创建剩下的对象
                for (int i = min_same_count; i < _data.Count; i++)
                {
                    TData vo_data = _data[i];
                    IScrollListItem<TData> item = _find_suit_item(vo_data, _item_factory, _item_cache_list);
                    if (item == null)
                        item = _item_factory.CreateScrollItem(vo_data);


                    item.ItemIndex = i;
                    item.SetClickCB(_on_item_click);
                    item.SetViewCreateCB(_on_view_create);
                    _item_list.Add(item);
                    _scroller.AddItem(item);
                    item.SetData(_data[i]);
                }

                //3. 删除不用的节点
                foreach (var item in _item_cache_list)
                {
                    item.Destroy();
                }

                _item_cache_list.ExtClear();
            }
            else
            {
                // 删除多余的 item 
                if (_item_list.Count >= _data.Count)
                {
                    //删除对象
                    int count_to_del = _item_list.Count - _data.Count;
                    for (int i = 0; i < count_to_del; ++i)
                    {
                        int index = _item_list.Count - 1;
                        var last_one = _item_list[index];
                        _item_list.RemoveAt(index);
                        _scroller.RemoveItem(last_one, true);
                    }
                }

                //添加不够的item
                for (int i = _item_list.Count; i < _data.Count; ++i)
                {
                    var item = _item_factory.CreateScrollItem(_data[i]);

                    item.ItemIndex = i;
                    item.SetClickCB(_on_item_click);
                    item.SetViewCreateCB(_on_view_create);
                    _item_list.Add(item);
                    _scroller.AddItem(item);
                    item.SetData(_data[i]);
                }
            }
            _scroller.EndBatch();
        }

        public IScrollListItem<TData> _find_suit_item(
            TData vo_data,
            IUIScrollListItemFactory<TData> item_factory,
            LinkedList<IScrollListItem<TData>> link_list)
        {
            //找到可以从用的item                
            LinkedListNode<IScrollListItem<TData>> item_node = null;
            for (var temp_node = link_list.First; temp_node != null; temp_node = temp_node.Next)
            {
                bool is_suite = item_factory.IsScrollItemSuite(temp_node.Value, vo_data);
                if (is_suite)
                {
                    item_node = temp_node;
                    break;
                }
            }

            if (item_node == null)
                return null;
            IScrollListItem<TData> ret = item_node.Value;
            link_list.ExtRemove(item_node);
            item_node.Value = null;
            return ret;
        }

        public void SetCompCreateCB(Action<TData, IUIView> cb)
        {
            _view_create_cb = cb;
        }

        public List<IScrollListItem<TData>> GetItemList()
        {
            return _item_list;
        }

        public void RefreshItemData(int index)
        {
            if (index < 0 || index >= _item_list.Count)
            {
                return;
            }

            IScrollListItem<TData> item = _item_list[index];
            TData data = _data[index];
            item.SetData(data);
        }

        public void Select(int index)
        {
            for (int i = 0; i < _item_list.Count; i++)
            {
                _item_list[i].Selected = (i == index);
            }
            _scroller.Refresh(true);
        }

        public void SetItemClickCB(ScrollItemClickCB cb)
        {
            _item_click_cb = cb;
            if (cb != null)
                _item_factory.CheckViewClickable();
        }

        public void ClearItems()
        {
            SetTData(null);
        }

        public void Destroy()
        {
            ClearItems();
        }

        public void _on_item_click(int index, long user_data)
        {
            _item_click_cb?.Invoke(index, user_data);
        }

        public void _on_view_create(TData data, IUIView view)
        {
            _view_create_cb?.Invoke(data, view);
        }
    }
}

