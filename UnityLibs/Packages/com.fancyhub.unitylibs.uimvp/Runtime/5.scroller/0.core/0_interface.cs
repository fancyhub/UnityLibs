/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/27
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace FH.UI
{
    /// <summary>
    /// 滚动窗内Item控件的父节点
    /// 接收子节点size改变消息
    /// </summary>
    public interface IScrollItemParent
    {
        public RectTransform ItemParent { get; }
        public IResHolder Holder { get; }
        public void OnChildSizeChange();
    }

    /// <summary>
    /// 单个 scrollitem
    /// </summary>
    public interface IScrollItem
    {
        public void SetParent(IScrollItemParent parent);
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; }
        public Vector2 AnimPos { get; set; }
        public void Destroy();
        public bool CullVisible { get; set; }
    }

    /// <summary>
    /// 容器
    /// </summary>
    public interface IScroll : ICPtr
    {
        public ScrollRect UnityScroll { get; }

        public IScrollEvent ScrollerEvent { get; set; }

        /// <summary>
        /// 获取裁剪窗口的大小
        /// </summary>
        public Vector2 ViewSize { get; }

        /// <summary>
        /// 内容的位置
        /// </summary>
        public Vector2 ContentPos { get; set; }

        public Vector2 ContentSize { get; set; }

        /// <summary>
        /// 返回 Content Pos的范围
        /// x_max,y_max  ,因为最小值都是0,0
        /// </summary>
        public Vector2 ContentPosMax { get; }


        /// <summary>
        /// 会阻止 ItemChange  的事件
        /// </summary>
        public int BeginBatch();

        public void AddItem(IScrollItem item);
        public void RemoveItem(IScrollItem item, bool destroy_item);
        public List<IScrollItem> GetItemList();

        /// <summary>
        /// 会destroy  items
        /// </summary>
        public void ClearItems();

        public int EndBatch();
    }

    public struct ScrollLayoutPadding
    {
        public static ScrollLayoutPadding Zero = new ScrollLayoutPadding(0, 0, 0, 0);

        public int Left;
        public int Right;

        public int Top;
        public int Bottom;

        public ScrollLayoutPadding(int left, int right, int top, int bottom)
        {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }

        public bool IsEqual(ScrollLayoutPadding other)
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

        public static ScrollLayoutPadding From(RectOffset offset)
        {
            return new ScrollLayoutPadding(offset.left, offset.right, offset.top, offset.bottom);
        }
    }     

    [Flags]
    public enum EScrollLayoutBuildFlag
    {
        None = 0,
        ItemChange = 1,
        Moving = 2,
        ViewSizeChange = 4,
    }

    /// <summary>
    /// 布局类的接口
    /// </summary>
    public interface IScrollLayout
    {
        public void Build(IScroll scroller);

        public EScrollLayoutBuildFlag BuildFlag { get; }

        public bool EdChanged();
    }

    public interface IScrollCuller
    {
        /// <summary>
        /// false: 说明，有item 在 culling visible == true 之后，size 发生了变化，会中断当前的 culling，返回到外面，重新排序
        /// true: 没有任何变化
        /// </summary>
        public bool Cull(IScroll scroller);
    }

    public interface IScrollMovement
    {
        public float Threshold { get; set; }
        public void StopMovement();
        public IScrollEvent ScrollerEvent { get; set; }
    }


    public interface IScrollEvent
    {
        public void OnScrollDragStart(); // 开始移动了
        public void OnScrollDragEnd(); //拖拽结束了
        public void OnScrollMoving();  //移动中
        public void OnScrollMoveEnd();//移动结束了
        public void OnScrollUpdate();

        public void OnScrollViewSizeChange(Vector2 dt);
        public void OnScrollItemChange();
    }
}
