/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FH.UI
{
    /// <summary>
    /// Scroll 的容器类
    /// 1. 添加/删除 Item
    /// 2. 获取/设置 ViewPort的size
    /// 3. 获取/设置 Content Size
    /// 4. 获取/设置 Content Pos
    /// </summary>
    internal class UIScroll : IScroll, IScrollItemParent
    {
        public int ObjVersion { get; private set; }

        public struct ScrollerDummy
        {
            public RectTransform _Tran;

            public RectTransform Get(ScrollRect scroll_rect)
            {
                if (_Tran == null)
                {
                    GameObject obj = new GameObject("");
                    obj.layer = scroll_rect.gameObject.layer;
                    obj.transform.SetParent(scroll_rect.content, false);
                    _Tran = obj.AddComponent<RectTransform>();
                    _Tran.pivot = Vector2.one * 0.5f;
                    _Tran.anchorMin = new Vector2(0, 1);
                    _Tran.anchorMax = new Vector2(0, 1);
                    _Tran.localPosition = Vector3.zero;
                    _Tran.sizeDelta = Vector2.zero;
                }
                return _Tran;
            }

            public void Destroy()
            {
                if (_Tran == null)
                    return;
                GameObject.Destroy(_Tran);
                _Tran = null;
            }
        }

        private IResInstHolder _holder;
        private ScrollRect _unity_scroll;
        private RectTransform _view_port_tran;
        private RectTransform _content_tran;
        private ScrollerDummy _dummy;
        private Vector2 _view_port_size;

        private Vector2 _content_size = Vector2.zero;
        private IScrollEvent _evt;
        private List<IScrollItem> _item_list = new List<IScrollItem>();
        private int _batch_mode = 0;

        public UIScroll(ScrollRect scroll_rect, IResInstHolder holder)
        {
            _holder = holder;

            _unity_scroll = scroll_rect;
            _view_port_tran = scroll_rect.viewport;
            _content_tran = _unity_scroll.content;

            ContentPos = Vector2.zero;
            ContentSize = Vector2.zero;
            _view_port_size = _view_port_tran.rect.size;
            ViewPortSizeMonitor.MonitorSizeChanged(_view_port_tran, _on_view_size_changed);
        }

        public IScrollEvent ScrollerEvent { get { return _evt; } set { _evt = value; } }

        public ScrollRect UnityScroll { get { return _unity_scroll; } }

        #region 
        public Vector2 ViewSize
        {
            get
            {
                Vector2 size = _view_port_tran.rect.size;
                return size;
            }            
        }

        private void _on_view_size_changed()
        {
            //1. 获取size, 不能变成负数
            Vector2 new_size = _view_port_tran.rect.size;

            //2. 比较,是否发生变化
            Vector2 old_size = _view_port_size;
            if (_view_port_size.Equals(new_size))
                return;

            //3. 调整 content size
            _update_unity_content_size();

            //4. 发出事件, view size 发生变化了
            _evt?.OnScrollViewSizeChange(new_size - old_size);
        }
            
        public Vector2 ContentSize
        {
            set
            {
                Vector2 new_content_size = Vector2.Max(value, Vector2.zero);
                if (_content_size.Equals(new_content_size))
                    return;
                _content_size = new_content_size;
                _update_unity_content_size();
            }
            get
            {
                return _content_size;
            }
        }

        public Vector2 ContentPos
        {
            get
            {
                Vector2 ret = _content_tran.localPosition;
                ret.x = -ret.x;
                return ret;
            }
            set
            {
                Vector3 pos = value;
                pos.z = 0;
                pos.x = -pos.x;
                _content_tran.localPosition = pos;
            }
        }

        public Vector2 ContentPosMax
        {
            get
            {
                //根据 容器的大小来计算,而不是根据 内容的大小
                Vector2 unity_cont_size = _content_tran.rect.size;
                Vector2 view_size = ViewSize;

                Vector2 ret = unity_cont_size - view_size;
                ret.x = Mathf.Max(ret.x, 0.0001f);
                ret.y = Mathf.Max(ret.y, 0.0001f);

                return ret;
            }
        }

        private void _update_unity_content_size()
        {
            //1. 获取view 的size
            Vector2 new_content_transform_size = Vector2.Max(ViewSize, _content_size);
            Vector2 old_content_transform_size = _content_tran.rect.size;

            //4. 比较content size 是否发生了变化
            if (new_content_transform_size.Equals(old_content_transform_size))
                return;

            //5. 调整content size
            _content_tran.sizeDelta = new_content_transform_size;
        }
        #endregion         


        #region Items
        public int BeginBatch()
        {
            //Debug.Log("Batch Begin");
            _batch_mode++;
            return _batch_mode;
        }

        public void AddItem(IScrollItem item)
        {
            Log.Assert(!_item_list.Contains(item), "item list duplicate item !!!");
            _item_list.Add(item);
            item.SetParent(this);

            _item_changed();
        }

        /// <summary>
        /// 只有找到了之后才会destroy item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="destroy_item"></param>
        public void RemoveItem(IScrollItem item, bool destroy_item)
        {
            if (!_item_list.Remove(item))
                return;

            item.SetParent(null);
            if (destroy_item)
                item.Destroy();

            _item_changed();
        }

        public List<IScrollItem> GetItemList()
        {
            return _item_list;
        }

        public void ClearItems()
        {
            _clear_items();

            _item_changed();
        }

        public int EndBatch()
        {
            //Debug.Log("Batch End");
            if (_batch_mode <= 0)
                return _batch_mode;
            _batch_mode--;
            if (_batch_mode > 0)
                return _batch_mode;
            _batch_mode = 0;

            _item_changed();
            return _batch_mode;
        }

        private void _clear_items()
        {
            if (_item_list.Count == 0)
                return;

            foreach (var a in _item_list)
            {
                a.SetParent(null);
                a.Destroy();
            }

            _item_list.Clear();
        }

        private void _item_changed()
        {
            if (_batch_mode > 0)
                return;

            _evt?.OnScrollItemChange();
        }
        #endregion


        public void Destroy()
        {
            _clear_items();
            if (_unity_scroll != null)
                _unity_scroll.onValueChanged.RemoveAllListeners();


            _dummy.Destroy();
            ObjVersion++;
        }

        #region IScrollItemParent
        RectTransform IScrollItemParent.ItemParent
        {
            get
            {
                //return _dummy.Get(_unity_scroll);
                return _content_tran;
            }
        }

        IResInstHolder IScrollItemParent.Holder
        {
            get { return _holder; }
        }

        void IScrollItemParent.OnChildSizeChange()
        {
            _item_changed();
        }
        #endregion


        private class ViewPortSizeMonitor : UIBehaviour
        {
            private Action _call_back;
            public static void MonitorSizeChanged(RectTransform tar, Action call_back)
            {
                if (tar == null)
                    return;
                var inst = tar.GetComponent<ViewPortSizeMonitor>();
                if (inst == null)
                {
                    inst = tar.gameObject.AddComponent<ViewPortSizeMonitor>();
                }

                inst._call_back = call_back;
            }

            protected override void OnRectTransformDimensionsChange()
            {
                _call_back?.Invoke();
            }
        }
    }
}
