/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;


namespace FH.UI
{
    //垂直
    public class ScrollLayoutV : IScrollLayout
    {
        //item 和 item 之间的间隔
        public float _spacing = 0;

        //item 的x坐标在整个scroll view 里面的 对齐方式
        public float _alignment = 0.5f;

        //
        public ScrollLayoutPadding _padding = ScrollLayoutPadding.Zero;

        public ScrollLayoutV()
        {
        }

        public EScrollLayoutBuildFlag BuildFlag => EScrollLayoutBuildFlag.ItemChange;

        public void SetSpacing(float spacing)
        {
            _spacing = spacing;
        }

        public void SetAlignment(float alignment)
        {
            _alignment = alignment;
        }

        public void SetPadding(ScrollLayoutPadding padding)
        {
            _padding = padding;
        }

        public bool EdChanged() { return false; }

        public void Build(IScroll scroller)
        {
            //1. 参数检查
            if (null == scroller)
                return;
            Vector2 view_size = scroller.ViewSize;

            //2.1 初始化 x,y, 垂直方向高度
            float pos_y = _padding.Top;
            float size_y = _padding.Top;
            float view_width = view_size.x - _padding.Left - _padding.Right;

            //2.2 遍历所有item，一行一个item，从上往下排, y += item_size,
            foreach (IScrollItem item in scroller.GetItemList())
            {
                //2.2.1 给item赋值坐标
                Vector2 item_size = item.Size;
                float pos_x = (view_width - item_size.x) * _alignment + _padding.Left;
                item.Pos = new Vector2(pos_x, pos_y);

                //2.2.2 y坐标增加一个item高度，x坐标不变
                size_y = pos_y + item_size.y;
                pos_y = size_y + _spacing;
            }

            //3. 通过内容高度，重新设置内容大小
            size_y += _padding.Bottom;
            scroller.ContentSize = new Vector2(view_size.x, size_y);
        }
    }
}
