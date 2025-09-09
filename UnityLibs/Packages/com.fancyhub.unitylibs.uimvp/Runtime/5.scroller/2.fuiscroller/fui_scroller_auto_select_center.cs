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
    /// <summary>
    /// 移动结束后, 自动选择中间的Item
    /// </summary>
    public class FUIScrollerAutoSelectCenter
    {
        private FUIScroll _scroller;

        //item变化通知事件
        private Action<int> _on_item_change;

        //scroller居中的item索引
        private int _scroller_item_index = -1;

        private bool _dir_ver = true;


        public FUIScrollerAutoSelectCenter(FUIScroll scroller, Action<int> on_item_change)
        {
            _scroller = scroller;

            //设置item变化的事件通知
            _on_item_change = on_item_change;

            if (!scroller.UnityScroll.vertical)
                _dir_ver = false;

            //如果item变化没有事件通知，则不设置item居中的索引
            if (null != on_item_change)
            {
                _scroller.EventMoveEnd += _stopped_move;

                //有通知则设置当前选中的item索引
                _scroller_item_index = GetSelectedItemIndex();
            }
        }

        //scroller停止滑动的事件
        private void _stopped_move()
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
            if (_dir_ver)
            {
                //1. 找到Scroller scroller 的中心点
                //scroller 的中心点y坐标 = scroller 中内容位置的垂直坐标y + viewport的垂直长度的一半；
                float center_y = _scroller.ContentPos.y + _scroller.ViewSize.y * 0.5f;

                //2. 找到中心点被哪个item
                List<IScrollItem> item_list = _scroller.GetItemList();
                for (int i = 0; i < item_list.Count; ++i)
                {
                    IScrollItem item = item_list[i];
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
            else
            {
                //1. 找到Scroller scroller 的中心点
                //scroller 的中心点y坐标 = scroller 中内容位置的垂直坐标x + viewport的垂直长度的一半；
                float center_x = _scroller.ContentPos.x + _scroller.ViewSize.x * 0.5f;

                //2. 找到中心点被哪个item
                List<IScrollItem> item_list = _scroller.GetItemList();
                for (int i = 0; i < item_list.Count; ++i)
                {
                    IScrollItem item = item_list[i];
                    //2.1 每个item顶部的y坐标
                    float item_top_x = item.Pos.x;
                    //2.2 底部的y坐标 = 顶部y坐标 + item垂直高度
                    float item_bot_x = item_top_x + item.Size.x;
                    //2.3 如果这个item的顶部坐标 和底部坐标组成的区间，包含了 scroller 的中心点
                    //则说明这个item在 scroller 的中间位置
                    if (item_top_x <= center_x && center_x < item_bot_x)
                    {
                        return i;
                    }
                }
                return -1;
            }

        }
    }
}
