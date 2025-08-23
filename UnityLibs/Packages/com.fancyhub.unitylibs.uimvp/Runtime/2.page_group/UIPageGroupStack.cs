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

        public EUIPageGroupType GroupType => EUIPageGroupType.Stack;

        public EUIPageGroupChannel Channel { get; internal set; }

        public UIPageGroupStack(EUIPageGroupChannel channel)
        {
            Channel = channel;
        }

        public void ClearEmpty()
        {
            _stack.ClearEmpty();
        }

        public bool AddPage(IUIGroupPage page)
        {
            if (page == null)
                return false;

            var topNode = _stack.GetFirst();
            _stack.AddFirst(page);

            if (topNode != null)
                topNode.Value.Val.SetPageGroupVisible(false);
            page.SetPageGroupVisible(true);
            return true;
        }

        public bool RemovePage(int pageId)
        {
            LinkedListNode<CPtr<IUIGroupPage>> node = _stack.Find(pageId);
            if (node == null) //被移除的Page 已经被销毁了
            {
                LinkedListNode<CPtr<IUIGroupPage>> topNode = _stack.GetFirst();
                if (topNode != null)
                    topNode.Value.Val.SetPageGroupVisible(true);
                return true;
            }
            else
            {
                //node.Value.Val.UIHide();//? 不需要这里调用
                LinkedListNode<CPtr<IUIGroupPage>> topNode = _stack.GetFirst();
                bool isCurrent = node == topNode;
                node.ExtRemoveFromList();

                if (!isCurrent)
                {
                    UILog._.D("Page:{0} , is not top of stack group", pageId);
                    return true;
                }

                topNode = _stack.GetFirst();
                if (topNode != null)
                    topNode.Value.Val.SetPageGroupVisible(true);
                return true;
            }
        }

        public T ShowPage<T>() where T : class, IUIGroupPage
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
