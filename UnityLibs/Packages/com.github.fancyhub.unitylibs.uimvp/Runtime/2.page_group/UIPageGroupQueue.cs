/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/6/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


namespace FH.UI
{
    sealed class UIPageGroupQueue : IUIPageGroup
    {
        private UIPageList _queue = new UIPageList();

        public EUIPageGroupType GroupType => EUIPageGroupType.Queue;

        public EUIPageGroupChannel Channel { get; internal set; }

        public UIPageGroupQueue(EUIPageGroupChannel channel)
        {
            Channel = channel;
        }

        public bool AddPage(IUIGroupPage page)
        {
            if (page == null)
                return false;
            bool shouldShow = _queue.Count == 0;
            _queue.AddLast(page);

            page.SetPageGroupVisible(shouldShow);
            return true;
        }

        public bool RemovePage(int pageId)
        {
            var node = _queue.Find(pageId);
            if (node == null) //被移除的page, 已经被销毁了
            {
                var firstNode = _queue.GetFirst();
                if (firstNode != null)
                    firstNode.Value.Val.SetPageGroupVisible(true);
                return true;
            }
            else
            {
                //node.Value.Val.UIHide();//不调用了

                //判断是否为第一个
                var firstNode = _queue.GetFirst();
                node.ExtRemoveFromList();
                if (node != firstNode)// 不是第一个, 直接返回            
                    return true;

                //再次获取第一个
                firstNode = _queue.GetFirst();
                if (firstNode != null)
                    firstNode.Value.Val.SetPageGroupVisible(true);
                return true;
            }
        }

        public void ClearEmpty()
        {
            _queue.ClearEmpty();
        }

        public T ShowPage<T>() where T : class, IUIGroupPage
        {
            //1. 找到该节点
            var node = _queue.FindFirst<T>();
            if (node == null)
                return null;
            T ret = node.Value.Val as T;

            //2. 找到当前显示的节点
            var currentNode = _queue.GetFirst();
            if (currentNode == node) //如果就是当前节点
                return ret;

            //3. 把要显示的节点移到最前面
            node.ExtMove2First();

            //4. 隐藏旧的,显示新的
            currentNode.Value.Val.SetPageGroupVisible(false);
            node.Value.Val.SetPageGroupVisible(true);
            return ret;
        }
    }
}
