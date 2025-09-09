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
    public class ScrollLayoutCenterV : IScrollLayout
    {
        //item间隔
        private float _spacing;

        public ScrollLayoutCenterV()
        {
        }

        public ScrollLayoutCenterV(float spacing)
        {
            _spacing = spacing;
        }

        public EScrollLayoutBuildFlag BuildFlag => EScrollLayoutBuildFlag.ItemChange;

        public bool EdChanged() { return false; }

        public void Build(IScroll scroller)
        {
            //1. 参数检查 
            if (null == scroller)
                return;

            //2.1 获取所有itemlist,如果没有item，内容大小为0
            var item_list = scroller.GetItemList();
            if (0 == item_list.Count)
            {
                scroller.ContentSize = new Vector2(0, 0);
                return;
            }

            //2.2 如果有item 找到item一半的大小
            float item_size = item_list[0].Size.y * 0.5f;
            //2.3 设置y坐标为 视口高度的一半 - 一半item大小 
            float y = scroller.ViewSize.y * 0.5f - item_size;
            const float x = 0;
            float size_y = 0;

            //3. 遍历所有item
            foreach (var a in scroller.GetItemList())
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
            size_y += scroller.ViewSize.y * 0.5f - item_size;
            //5. 重新设置内容大小
            scroller.ContentSize = new Vector2(x, size_y);
        }
    }
}
