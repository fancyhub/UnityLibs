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
    /// <summary>
    /// 用来culling
    /// 1. 当 Item 发生变化
    /// 2. 当 Scroll 移动的时候
    /// 3. 当 Scroll 的裁剪 区域大小变化的时候
    /// </summary>
    public class ScrollItemCulling : IScrollerCuller
    {
        public bool _in_stack = false;
        public List<IScrollerItem> _list_to_invisible = new List<IScrollerItem>();
        public IScroller Scroller { get; set; }

        public ScrollItemCulling()
        {
        }

        public bool Cull()
        {
            //防止重入
            if (_in_stack)
                return true; //没有发生变化

            _in_stack = true;
            _list_to_invisible.Clear();

            Vector2 size = Scroller.ViewSize;
            Vector2 pos = Scroller.ContentPos;

            Rect rect = new Rect(pos, size);

            var item_list = Scroller.GetItemList();
            bool ret = true; //默认没有变化

            //先把 invisble 的对象 变成 visible，最后处理 要隐藏的对象
            foreach (var a in item_list)
            {
                Vector2 item_pos = a.Pos + a.AnimPos;
                Vector2 item_size = a.Size;
                bool visible = rect.Overlaps(new Rect(item_pos, item_size));
                if (a.CullVisible == visible)
                    continue;

                if (visible)
                {
                    a.CullVisible = true;
                    if (a.Size != item_size)
                    {
                        //发生了变化
                        ret = false;
                        break;
                    }
                    continue;
                }
                _list_to_invisible.Add(a);
            }

            foreach (var a in _list_to_invisible)
            {
                a.CullVisible = false;
            }
            _in_stack = false;
            return ret;
        }
    }
}
