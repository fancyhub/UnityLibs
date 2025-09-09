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
    public interface IScrollerItemParent
    {
        public RectTransform ItemParent { get; }
        public IResInstHolder Holder { get; }
        public void OnChildSizeChange();
    }

    /// <summary>
    /// 单个 scrollitem
    /// </summary>
    public interface IScrollerItem
    {
        public void SetParent(IScrollerItemParent parent);
        public Vector2 Pos { get; set; }
        public Vector2 Size { get; }
        public Vector2 AnimPos { get; set; }
        public void Destroy();
        public bool CullVisible { get; set; }
    }

    public interface IScrollerEvent
    {
        public void OnScrollDragStart(); // 开始移动了
        public void OnScrollDragEnd(); //拖拽结束了
        public void OnScrollMoving();  //移动中
        public void OnScrollMoveEnd();//移动结束了
        public void OnScrollViewSizeChange(Vector2 dt);
        public void OnScrollItemChange();
        public void OnScrollUpdate();
    }

    /// <summary>
    /// 容器
    /// </summary>
    public interface IScroller : ICPtr
    {
        public ScrollRect UnityScroll { get; }

        public IScrollerEvent ScrollEvent { get; set; }

        /// <summary>
        /// 获取裁剪窗口的大小
        /// </summary>
        public Vector2 ViewSize { get; set; }

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


        public void StopMovement();

        /// <summary>
        /// 会阻止 ItemChange  的事件
        /// </summary>
        public int BeginBatch();

        public void AddItem(IScrollerItem item);
        public void RemoveItem(IScrollerItem item, bool destroy_item);
        public List<IScrollerItem> GetItemList();

        /// <summary>
        /// 会destroy  items
        /// </summary>
        public void ClearItems();

        public int EndBatch();
    }

    /// <summary>
    /// 布局类的接口
    /// </summary>
    public interface IScrollerLayout
    {
        public IScroller Scroller { set; }
        public void Build();

        public bool EdChanged();
    }

    public interface IScrollerCuller
    {
        public IScroller Scroller { get; set; }

        /// <summary>
        /// false: 说明，有item 在 culling visible == true 之后，size 发生了变化，会中断当前的 culling，返回到外面，重新排序
        /// true: 没有任何变化
        /// </summary>
        public bool Cull();
    }
}
