/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    //水平方向
    public class LayoutH : IScrollerLayout
    {
        public IScroller Scroller { set; get; }

        //每个元素之间的间距
        private float _spacing = 0;

        //item 的 y 坐标在整个scroll view 里面的 对齐方式
        private float _alignment = 0.5f;

        private LayoutPadding _padding = LayoutPadding.Zero;

        public LayoutH()
        {
        }

        public LayoutH(float spacing)
        {
            _spacing = spacing;
        }

        public float Alignment
        {
            get => _alignment;
            set => _alignment = Mathf.Clamp01(value);
        }


        public float Spacing
        {
            get => _spacing;
            set => _spacing = Mathf.Max(0, value);
        }

        public LayoutPadding Padding
        {
            get => _padding;
            set => _padding = value;
        }

        public bool EdChanged() { return false; }

        public void Build()
        {
            //1. 参数检查
            if (null == Scroller)
                return;
            Vector2 view_size = Scroller.ViewSize;

            //2.1 初始化参数 : x,y游标，一行的宽度size_x
            float pos_x = _padding.Left;
            float size_x = _padding.Left;
            float view_height = view_size.y - _padding.Top - _padding.Bottom;

            //2.2 遍历item_list
            foreach (IScrollerItem item in Scroller.GetItemList())
            {
                Vector2 item_size = item.Size;

                //2.3 设置item位置
                float pos_y = (view_height - item_size.y) * _alignment + _padding.Top;
                item.Pos = new Vector2(pos_x, pos_y);

                //2.4 x游标右移
                size_x = pos_x + item_size.x;
                pos_x = size_x + _spacing;
            }

            //3. 重置内容大小
            size_x += _padding.Right;
            Scroller.ContentSize = new Vector2(size_x, view_size.y);
        }
    }
}
