/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using System.Collections.Generic;


namespace FH.UI
{
    // 垂直移动，但是 水平方向是按照grid 来的    
    public class LayoutVRow : IScrollerLayout
    {
        public IScroller Scroller { get; set; }

        private int _count;

        //每个cell 的alignment
        private float _alignment = 0.5f;

        //y 方向上 两个item的间距
        private float _spacing = 0;

        private LayoutPadding _padding = LayoutPadding.Zero;

        public LayoutVRow(int count = 1)
        {
            _count = Mathf.Max(1, count);
        }

        public float Alignment
        {
            get => _alignment;
            set => _alignment = Mathf.Clamp01(value);
        }

        public int Count
        {
            get => _count;
            set => _count = Mathf.Max(1, value);
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

            //2. 一行中的每个单元的宽度，（视口宽度 / item个数）
            List<IScrollerItem> list = Scroller.GetItemList();
            Vector2 view_size = Scroller.ViewSize;
            float cell_width = (view_size.x - _padding.Left - _padding.Right) / _count;

            //3. 初始化y坐标，下一个y的坐标，一行的item计数
            float pos_y = _padding.Top;
            float size_y = _padding.Top;
            int x_count = 0;

            //4. 遍历item_list
            for (int i = 0; i < list.Count; ++i)
            {
                //4.1 i与每行的item计数求模，保证i不超过每行的item个数
                int x = i % _count;

                //4.2 设置该index下item的位置坐标
                IScrollerItem item = list[i];
                Vector2 item_size = item.Size;
                float pos_x = x * cell_width + (cell_width - item_size.x) * _alignment + _padding.Left;
                item.Pos = new Vector2(pos_x, pos_y);

                //4.3 下一个y坐标为当前y坐标加上一个item的高度
                size_y = Mathf.Max(size_y, pos_y + item_size.y);

                //4.4 如果行计数达到上限
                x_count++;
                if (x_count == _count)
                {
                    //4.5.1 计数规0
                    x_count = 0;

                    //4.5.2 当前y坐标更改为下一行的y坐标
                    pos_y = size_y + _spacing;
                }
            }

            //5. 计算完所有item后得出的next_pos_y则为内容大小的高度
            size_y += _padding.Bottom;
            Scroller.ContentSize = new Vector2(view_size.x, size_y);
        }
    }
}
