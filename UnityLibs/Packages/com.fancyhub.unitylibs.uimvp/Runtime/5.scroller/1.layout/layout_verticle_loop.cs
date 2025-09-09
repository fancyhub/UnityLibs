/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/2 11:35:47
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    //垂直, 循环的, 必须每个item的大小一致
    public class ScrollLayoutVLoop : IScrollLayout
    {
        public float _spacing = 0;
        public bool _half_view_offset = false;

        public ScrollLayoutVLoop()
        {
        }

        public ScrollLayoutVLoop(float spacing, bool half_view_offset)
        {
            _spacing = spacing;
            _half_view_offset = half_view_offset;
        }

        public EScrollLayoutBuildFlag BuildFlag => EScrollLayoutBuildFlag.ItemChange| EScrollLayoutBuildFlag.Moving;

        public bool EdChanged() { return false; }

        public void Build(IScroll scroller)
        {
            //1. 参数检查
            if (null == scroller)
                return;

            List<IScrollItem> item_list = scroller.GetItemList();
            int item_count = item_list.Count;
            if (0 == item_count)
                return;

            float item_size = item_list[0].Size.y;
            if (item_size <= 0.01f)
                return;

            item_size = item_size + _spacing;
            float view_size = scroller.ViewSize.y;

            float center_y = view_size * 0.5f + scroller.ContentPos.y;

            float offset = 0;
            if (_half_view_offset)
            {
                offset = view_size * 0.5f - item_size * 0.5f;
            }

            int center_item_index = Mathf.RoundToInt((center_y - offset) / item_size);
            float center_item_pos = center_item_index * item_size;
            center_item_index = center_item_index % item_count;

            int start_item_index = center_item_index - item_count / 2;
            float start_item_pos = center_item_pos - (item_count / 2) * item_size + offset;

            for (int i = 0; i < item_count; i++)
            {
                int item_index = (i + start_item_index) % item_count;
                if (item_index < 0)
                    item_index += item_count;
                IScrollItem item = item_list[item_index];

                float y = item_size * i + 0.5f * _spacing + start_item_pos;
                item.Pos = new Vector2(0, y);
            }
        }
    }
}
