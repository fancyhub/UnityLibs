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
    public struct LayoutPadding
    {
        public static LayoutPadding Zero = new LayoutPadding(0, 0, 0, 0);

        public int Left;
        public int Right;

        public int Top;
        public int Bottom;

        public LayoutPadding(int left, int right, int top, int bottom)
        {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }

        public bool IsEqual(LayoutPadding other)
        {
            if (other.Left != Left)
                return false;
            if (other.Right != Right)
                return false;
            if (other.Top != Top)
                return false;
            if (other.Bottom != Bottom)
                return false;
            return true;
        }

        public static LayoutPadding From(RectOffset offset)
        {
            return new LayoutPadding(offset.left, offset.right, offset.top, offset.bottom);
        }
    }

    //垂直
    public class LayoutV : IScrollerLayout
    {
        public IScroller Scroller { set; get; }

        //item 和 item 之间的间隔
        public float _spacing = 0;

        //item 的x坐标在整个scroll view 里面的 对齐方式
        public float _alignment = 0.5f;

        //
        public LayoutPadding _padding = LayoutPadding.Zero;

        public LayoutV()
        {
        }

        public void SetSpacing(float spacing)
        {
            _spacing = spacing;
        }

        public void SetAlignment(float alignment)
        {
            _alignment = alignment;
        }

        public void SetPadding(LayoutPadding padding)
        {
            _padding = padding;
        }
        public bool EdChanged() { return false; }
        public void Build()
        {
            //1. 参数检查
            if (null == Scroller)
                return;
            Vector2 view_size = Scroller.ViewSize;

            //2.1 初始化 x,y, 垂直方向高度
            float pos_y = _padding.Top;
            float size_y = _padding.Top;
            float view_width = view_size.x - _padding.Left - _padding.Right;

            //2.2 遍历所有item，一行一个item，从上往下排, y += item_size,
            foreach (IScrollerItem item in Scroller.GetItemList())
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
            Scroller.ContentSize = new Vector2(view_size.x, size_y);
        }
    }
}
