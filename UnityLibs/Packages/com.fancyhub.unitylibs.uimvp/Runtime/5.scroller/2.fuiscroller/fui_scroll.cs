/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/27 14:43:56
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace FH.UI
{

    /// <summary>
    /// 一个简单的组装
    /// </summary>
    public class FUIScroll : IScrollEvent, ICPtr
    {
        public event Action EventDragStart; // 开始移动了
        public event Action EventDragEnd; //拖拽结束了        
        public event Action EventMoveEnd; //移动结束了

        public int ObjVersion { get; private set; }

        private IScroll _scroller;
        private IScrollLayout _layout;
        private IScrollCuller _culler;
        private IScrollMovement _movement;

        private bool _in_stack = false;

        public static FUIScroll Create(ScrollRect scroll_rect, IResInstHolder res_holder)
        {
            FUIScroll eui_scroll = new FUIScroll();
            eui_scroll._scroller = new UIScroll(scroll_rect, res_holder);
            eui_scroll._scroller.ScrollerEvent = eui_scroll;
            eui_scroll._movement = UIScrollMovement.Get(scroll_rect);
            eui_scroll._movement.ScrollerEvent = eui_scroll;
            eui_scroll._culler = new ScrollCuller();

            return eui_scroll;
        }

        public IScrollLayout Layout
        {
            get
            {
                if (_layout == null)
                    _layout = ScrollLayoutUnityWrapFactory.Create(_scroller.UnityScroll, _scroller.UnityScroll.content);
                return _layout;
            }

            set
            {
                _layout = value;
            }
        }

        private FUIScroll()
        {
        }

        public IScrollMovement Movement => _movement;

        public Vector2 ViewSize
        {
            get => _scroller.ViewSize;
        }

        public Vector2 ContentSize
        {
            get => _scroller.ContentSize;
            set => _scroller.ContentSize = value;
        }

        public Vector2 ContentPos
        {
            get => _scroller.ContentPos;
            set => _scroller.ContentPos = value;
        }

        public int BeginBatch()
        {
            return _scroller.BeginBatch();
        }

        public int EndBatch()
        {
            return _scroller.EndBatch();
        }

        public ScrollRect UnityScroll
        {
            get => _scroller.UnityScroll;
        }

        public void Refresh(bool layout = false)
        {
            _build_layout_and_culling(layout);
        }

        public void Destroy()
        {
            _scroller.Destroy();
            ObjVersion++;
        }

        public void AddItem(IScrollItem item)
        {
            _scroller.AddItem(item);
        }

        public void RemoveItem(IScrollItem item)
        {
            _scroller.RemoveItem(item, true);
        }

        public void RemoveItem(IScrollItem item, bool destroy_item)
        {
            _scroller.RemoveItem(item, destroy_item);
        }

        public void ClearItems()
        {
            _scroller.ClearItems();
        }

        public void ResetPosition()
        {
            _movement.StopMovement();
            _scroller.ContentPos = Vector2.zero;
        }

        public void MoveToHead()
        {
            _movement.StopMovement();
            _scroller.ContentPos = Vector2.zero;
        }

        public void MoveToEnd()
        {
            _movement.StopMovement();
            for (; ; )
            {
                _scroller.ContentPos = _scroller.ContentPosMax;

                Vector2 dt = _scroller.ContentPos - _scroller.ContentPosMax;
                if (dt.sqrMagnitude < 1)
                    break;
            }
        }

        public void MoveItemToViewport(IScrollItem item)
        {
            float percent;
            if (_find_nearest_percent(item, out percent))
            {
                MoveToItem(item, percent);
            }
        }

        public void MoveItemToViewport(int item_index)
        {
            IScrollItem item = _get_item_by_index(item_index);
            if (null != item)
            {
                MoveItemToViewport(item);
            }
        }

        //view_percent 描述item在viewport区域的百分比位置，vertical情况下，0为最上方
        public void MoveToItem(int item_index, float view_percent)
        {
            IScrollItem item = _get_item_by_index(item_index);
            if (null != item)
            {
                MoveToItem(item, view_percent);
            }
        }

        public void MoveToItem(IScrollItem item, float view_percent)
        {
            view_percent = Mathf.Clamp01(view_percent);

            Vector2 item_size = item.Size;
            Vector2 item_pos = item.Pos;

            Vector2 view_size = _scroller.ViewSize;
            Vector2 new_content_pos = item_pos - (view_size - item_size) * view_percent;

            _scroller.ContentPos = new_content_pos;
        }

        public List<IScrollItem> GetItemList()
        {
            return _scroller.GetItemList();
        }

        public bool IsAtTheEnd()
        {
            Vector2 pos = _scroller.ContentPos;
            Vector2 max_pos = _scroller.ContentPosMax;

            Vector2 dt = max_pos - pos;
            if (Mathf.Abs(dt.y) < 5)
                return true;
            return false;
        }

        private bool _find_nearest_percent(IScrollItem item, out float percent)
        {
            //获取item高度和垂直坐标
            float item_height = item.Size.y;
            float item_pos = item.Pos.y;

            //获取scroller视口的高度和垂直坐标
            float view_height = ViewSize.y;
            float view_pos = _scroller.ContentPos.y;

            //1. 如果item高度比视口高度还大，则放置item到scroller视口顶部
            if (item_height >= view_height)
            {
                percent = 0f;
                return true;
            }

            //2. 如果item底部的位置在视口的上方或相交，则放置item到scroller视口顶部
            if (item_pos < view_pos)
            {
                percent = 0f;
                return true;
            }

            float view_bottom_pos = view_pos + view_height;
            float item_bottom_pos = item_pos + item_height;
            //3. 如果item在视口下方或相交，则放置item到scroller视口的底部
            if (item_bottom_pos > view_bottom_pos)
            {
                percent = 1f;
                return true;
            }

            //4. item在视口中，不更新item位置
            percent = 0f;
            return false;
        }

        private IScrollItem _get_item_by_index(int item_index)
        {
            var items = GetItemList();
            if (item_index < 0 || item_index >= items.Count)
            {
                return null;
            }

            return items[item_index];
        }

        #region IScrollerEvent
        void IScrollEvent.OnScrollUpdate()
        {
#if UNITY_EDITOR 
            if (Layout.EdChanged())
            {
                this.Refresh(true);
            }
#endif
        }
        void IScrollEvent.OnScrollItemChange()
        {
            if (_in_stack)
                return;

            _build_layout_and_culling((Layout.BuildFlag & EScrollLayoutBuildFlag.ItemChange) != EScrollLayoutBuildFlag.None);
        }

        void IScrollEvent.OnScrollViewSizeChange(Vector2 dt)
        {
            Vector2 new_pos = _scroller.ContentPos + dt;
            Vector2 pos_max = _scroller.ContentPosMax;
            float pos_x = Mathf.Clamp(new_pos.x, 0, pos_max.x);
            float pos_y = Mathf.Clamp(new_pos.y, 0, pos_max.y);
            _scroller.ContentPos = new Vector2(pos_x, pos_y);


            _build_layout_and_culling((Layout.BuildFlag & EScrollLayoutBuildFlag.ViewSizeChange) != EScrollLayoutBuildFlag.None);
        }

        void IScrollEvent.OnScrollMoving()
        {
            _build_layout_and_culling((Layout.BuildFlag & EScrollLayoutBuildFlag.Moving) != EScrollLayoutBuildFlag.None);
        }

        void IScrollEvent.OnScrollDragStart()
        {
            EventDragStart?.Invoke();
        }

        //这里是drag end
        void IScrollEvent.OnScrollDragEnd()
        {
            EventDragEnd?.Invoke();
        }

        void IScrollEvent.OnScrollMoveEnd()
        {
            EventMoveEnd?.Invoke();
        }
        #endregion

        private void _build_layout_and_culling(bool rebuild)
        {
            if (_in_stack)
                return;
            _in_stack = true;

            if (rebuild)
                Layout.Build(_scroller);

            for (; ; )
            {
                if (_culler.Cull(_scroller))
                {
                    break;
                }
                Layout.Build(_scroller);
            }

            _in_stack = false;
        }
    }
}
