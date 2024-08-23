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

        public EUIPageGroup GroupType => EUIPageGroup.Queue;

        public void AddPage(IUIPageWithGroupInfo page)
        {
            if (page == null)
                return;
            bool shouldShow = _queue.Count == 0;
            _queue.AddLast(page);

                page.SetPageGroupVisible(shouldShow);            
        }

        public void RemovePage(int pageId)
        {
            var node = _queue.Find(pageId);
            if (node == null)
                return;
            //node.Value.Val.UIHide();//不调用了

            //判断是否为第一个
            var firstNode = _queue.GetFirst();
            node.ExtRemoveFromList();
            if (node != firstNode)// 不是第一个, 直接返回            
                return;

            //再次获取第一个
            firstNode = _queue.GetFirst();
            if (firstNode != null)
                firstNode.Value.Val.SetPageGroupVisible(true);
        }

        public void ClearEmpty()
        {
            _queue.ClearEmpty();
        }

        public IUIPageWithGroupInfo ShowPage<T>() where T : class, IUIPageWithGroupInfo
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
