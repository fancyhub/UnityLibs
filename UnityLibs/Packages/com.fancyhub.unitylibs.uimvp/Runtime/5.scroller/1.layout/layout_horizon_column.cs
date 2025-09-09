/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;


namespace FH.UI
{
    //水平移动，但是 竖直方向是按照grid 来的
    public class ScrollLayoutHCol : IScrollLayout
    {       
        //一行有多少列
        private int _count;
        private float _alignment = 0.5f;
        //x 方向上 两个item的间距
        private float _spacing = 0;
        protected ScrollLayoutPadding _padding = ScrollLayoutPadding.Zero;

        public ScrollLayoutHCol(int count = 1)
        {
            _count = Mathf.Max(1, count);
        }

        public EScrollLayoutBuildFlag BuildFlag => EScrollLayoutBuildFlag.ItemChange;

        public float Alignment
        {
            get => _alignment;
            set => _alignment = Mathf.Clamp01(value);
        }

        public int Count
        {
            get => _count;
            set =>_count = Mathf.Max(1, value);
        }

        public float Spacing
        {
            get => _spacing;
            set =>_spacing = Mathf.Max(0, value);
        }

        public ScrollLayoutPadding Padding
        {
            get => _padding;
            set => _padding = value;
        }
        public bool EdChanged()
        {
            return false;
        }
        //item在显隐过程中的重排序
        public void Build(IScroll scroller)
        {
            //1. 参数检查
            if (null == scroller)
                return;

            //2.1 获取 scroller 中的所有item
            var list = scroller.GetItemList();
            Vector2 view_size = scroller.ViewSize;

            //2.2 获取垂直方向每个单元的高度
            float cell_height = (view_size.y - _padding.Top - _padding.Bottom) / _count;

            //2.3 初始化水平坐标,item计数
            float pos_x = _padding.Left;
            float size_x = _padding.Left;
            int x_count = 0;

            //2.4 遍历item_list，item的索引与一行有多少列求模，保证索引不超过一行列数的上限
            for (int i = 0; i < list.Count; ++i)
            {
                int x = i % _count;
                IScrollItem item = list[i];
                Vector2 item_size = item.Size;

                //2.4.1 设置 item的坐标，x从0开始， y = x坐标 * 一个单元的高度
                float pos_y = x * cell_height + (cell_height - item_size.y) * _alignment + _padding.Top;
                item.Pos = new Vector2(pos_x, pos_y);

                //2.4.2 计算下一个x坐标
                size_x = Mathf.Max(size_x, pos_x + item_size.x);

                //2.4.3 如果列计数达到一行列数的上限，重置列计数，x坐标重置                
                x_count++;
                if (x_count == _count)
                {
                    x_count = 0;
                    pos_x = size_x + _spacing;
                }
            }

            //2.5 重置scoller内容高度
            size_x += _padding.Right;
            scroller.ContentSize = new Vector2(size_x, view_size.y);
        }
    }
}
