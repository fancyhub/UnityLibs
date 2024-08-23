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
    sealed class UIPageGroupStack : IUIPageGroup
    {
        //First 是栈顶
        private UIPageList _stack = new UIPageList();

        public EUIPageGroup GroupType => EUIPageGroup.Stack;

        public void ClearEmpty()
        {
            _stack.ClearEmpty();
        }

        public void AddPage(IUIPageWithGroupInfo page)
        {
            if (page == null)
                return;

            var topNode = _stack.GetFirst();
            _stack.AddFirst(page);

            if (topNode != null)
                topNode.Value.Val.SetPageGroupVisible(false);
            page.SetPageGroupVisible(true);
        }

        public void RemovePage(int pageId)
        {
            var node = _stack.Find(pageId);
            if (node == null)
                return;

            //node.Value.Val.UIHide();//? 不需要这里调用
            var topNode = _stack.GetFirst();
            bool isCurrent = node == topNode;
            node.ExtRemoveFromList();

            if (!isCurrent)
                return;

            topNode = _stack.GetFirst();
            if (topNode != null)
                topNode.Value.Val.SetPageGroupVisible(true);
        }

        public IUIPageWithGroupInfo ShowPage<T>() where T : class, IUIPageWithGroupInfo
        {
            var node = _stack.FindFirst<T>();
            if (node == null)
                return null;

            T ret = node.Value.Val as T;
            var topNode = _stack.GetFirst();
            if (topNode == node) //本身就是第一个
                return ret;

            node.ExtMove2First();
            topNode.Value.Val.SetPageGroupVisible(false);
            node.Value.Val.SetPageGroupVisible(true);
            return ret;
        }
    }
}
