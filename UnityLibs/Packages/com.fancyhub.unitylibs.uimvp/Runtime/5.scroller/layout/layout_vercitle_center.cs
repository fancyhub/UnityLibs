/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;


namespace FH.UI
{
    // 最好所有Item一样大，要不然会很麻烦
    public class LayoutCenterV : IScrollerLayout
    {
        public IScroller Scroller { set; get; }

        //item间隔
        public float _spacing;

        public LayoutCenterV()
        {
        }

        public LayoutCenterV(float spacing)
        {
            _spacing = spacing;
        }
        public bool EdChanged() { return false; }
        public void Build()
        {
            //1. 参数检查 
            if (null == Scroller)
                return;

            //2.1 获取所有itemlist,如果没有item，内容大小为0
            var item_list = Scroller.GetItemList();
            if (0 == item_list.Count)
            {
                Scroller.ContentSize = new Vector2(0, 0);
                return;
            }

            //2.2 如果有item 找到item一半的大小
            float item_size = item_list[0].Size.y * 0.5f;
            //2.3 设置y坐标为 视口高度的一半 - 一半item大小 
            float y = Scroller.ViewSize.y * 0.5f - item_size;
            const float x = 0;
            float size_y = 0;

            //3. 遍历所有item
            foreach (var a in Scroller.GetItemList())
            {
                //3.1 设置item的坐标
                a.Pos = new Vector2(x, y);
                //3.2 内容y坐标向下移动一个item高度，
                float item_size_y = a.Size.y;
                y += item_size_y;
                //3.3 计算内容高度size_y, 计算内容高度size_y += item_size.y
                size_y = y;

                //3.4 有大小的item才会计算padding。主要是为了解决tree view有item被折叠之后size为0的情况
                if (item_size_y > 1)
                    y += _spacing;
            }

            //4. 内容高度加上 (视口高度一半 - 一个item高度)
            size_y += Scroller.ViewSize.y * 0.5f - item_size;
            //5. 重新设置内容大小
            Scroller.ContentSize = new Vector2(x, size_y);
        }        
    }


    //垂直Scroller的中间选中item
    public class ScrollerCenterSelection
    {
        public EUIScroll _scroller;

        //item变化通知事件
        public Action<int> _on_item_change;

        //scroller居中的item索引
        public int _scroller_item_index = -1;

        #region Ctor

        public ScrollerCenterSelection(EUIScroll scroller)
        {
            _scroller = scroller;
        }

        #endregion

        public void SetItemChange(Action<int> on_item_change)
        {
            //设置item变化的事件通知
            _on_item_change = on_item_change;

            //如果item变化没有事件通知，则不设置item居中的索引
            if (null == on_item_change)
            {
                _scroller.EventMoveEnd -= _stopped_move;
            }
            else
            {
                _scroller.EventMoveEnd += _stopped_move;
                //有通知则设置当前选中的item索引
                _scroller_item_index = GetSelectedItemIndex();
            }
        }

        //scroller停止滑动的事件
        public void _stopped_move()
        {
            //1. 获取当前item索引
            int current_item = GetSelectedItemIndex();
            //2. 如果当前索引与scroller的居中索引一样则返回
            if (current_item == _scroller_item_index)
            {
                return;
            }

            //3. 如果不一样，设置居中索引为当前索引
            _scroller_item_index = current_item;
            //4. 通知item变化
            _on_item_change(_scroller_item_index);
        }

        public int GetSelectedItemIndex()
        {
            //1. 找到Scroller scroller 的中心点
            //scroller 的中心点y坐标 = scroller 中内容位置的垂直坐标y + viewport的垂直长度的一半；
            float center_y = _scroller.ContentPos.y + _scroller.ViewSize.y * 0.5f;

            //2. 找到中心点被哪个item
            List<IScrollerItem> item_list = _scroller.GetItemList();
            for (int i = 0; i < item_list.Count; ++i)
            {
                IScrollerItem item = item_list[i];
                //2.1 每个item顶部的y坐标
                float item_top_y = item.Pos.y;
                //2.2 底部的y坐标 = 顶部y坐标 + item垂直高度
                float item_bot_y = item_top_y + item.Size.y;
                //2.3 如果这个item的顶部坐标 和底部坐标组成的区间，包含了 scroller 的中心点
                //则说明这个item在 scroller 的中间位置
                if (item_top_y <= center_y && center_y < item_bot_y)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
