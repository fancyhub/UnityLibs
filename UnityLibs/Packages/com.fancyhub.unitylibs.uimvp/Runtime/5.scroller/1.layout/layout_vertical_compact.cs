/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    //紧凑垂直layout,尽量按一行一行排列，一行满就排到下一行
    public class ScrollLayoutVCompact : IScrollLayout
    { 
        //item 和item 之间 y 方向的距离
        public float _spacing_y = 0;

        public ScrollLayoutPadding _padding = ScrollLayoutPadding.Zero;

        //scroller 中的一行
        public LayoutVCompactRow _row = new LayoutVCompactRow();

        public ScrollLayoutVCompact(float spacing_y)
        {
            _spacing_y = spacing_y;
        }
        public EScrollLayoutBuildFlag BuildFlag => EScrollLayoutBuildFlag.ItemChange;
        public void SetSpacing(float spacing_x, float spacing_y)
        {
            _spacing_y = spacing_y;
            _row.SetSpacingX(spacing_x);
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

            //2.1 scroller 内容的y坐标初始化
            float y = _padding.Top;
            //2.2 找到空行
            _row.Clear();

            //2.3 设置一行的宽度为  scroller 内容宽度
            float width = scroller.ViewSize.x - _padding.Left - _padding.Right;

            _row.SetContentWidth(width);

            //2.4 遍历 scroller 的item_list， 按行排列，若一行排不下，换到下一行
            foreach (var a in scroller.GetItemList())
            {
                //2.4.1 如果一行加不进去说明行满
                if (_row.AddItem(a))
                {
                    continue;
                }

                //2.4.2 行满则重置x坐标到0
                _row.SetPosition(_padding.Left, y);

                //2.4.3 scroller 内容的y坐标增加一行的高度
                y += _row.GetHeight();

                //2.4.4 scroller 内容的y坐标增加padding
                y += _spacing_y;

                //2.4.5 清空row缓存，继续开始加入item
                _row.Clear();

                _row.AddItem(a);
            }

            //3.1 遍历结束后，row重置x坐标
            _row.SetPosition(_padding.Left, y);
            //3.2 内容y坐标增加一行的高度
            y += _row.GetHeight();
            //3.3 清理row缓存
            _row.Clear();


            //3.4 设置新的内容大小为内容宽度，和计算出来的y
            y += _padding.Bottom;
            scroller.ContentSize = new Vector2(width, y);
        }
    }

    public class LayoutVCompactRow
    {
        //一行中的item缓存
        public List<IScrollItem> _items = new List<IScrollItem>();

        //内容宽度
        public float _content_width;
        //行高
        public float _height;
        //行款
        public float _width;

        public float _spacing_x = 0;

        public void SetSpacingX(float x)
        {
            _spacing_x = x;
        }

        // 给 scroller 的一行加入item
        public bool AddItem(IScrollItem item)
        {

            //1. 每加入一个item，当前宽度就会加上一个item的宽度
            //判断当前加上这个item的宽度有没有超过内容宽度
            Vector2 item_size = item.Size;
            float current_width = _width + item_size.x;
            //2. 如果当前宽度大于了内容宽度，而且这一行中item数量不为0，则不能加入这一行
            if (current_width > _content_width && _items.Count != 0)
            {
                return false;
            }
            //3. 若当前行宽度还没到内容宽度 ,行的宽度增加
            _width += item_size.x;
            //4. item加入到list
            _items.Add(item);
            //5. 行高根据加入的item高度，选择最高的当做行高度。
            _height = Math.Max(_height, item_size.y);
            return true;
        }

        /// 设置行的位置
        public void SetPosition(float start_x, float start_y)
        {
            //1. 遍历这一行，设置item的坐标为 start_x, start_y, 
            foreach (IScrollItem item in _items)
            {
                item.Pos = new Vector2(start_x, start_y);
                //1.1 每设置一个坐标，start_x向右偏移一个item宽度的位置
                start_x += item.Size.x;
            }
        }

        public void Clear()
        {
            _items.Clear();
            _width = 0;
            _height = 0;
        }

        public void SetContentWidth(float content_width)
        {
            _content_width = content_width;
        }

        public float GetHeight()
        {
            return _height;
        }

    }
}