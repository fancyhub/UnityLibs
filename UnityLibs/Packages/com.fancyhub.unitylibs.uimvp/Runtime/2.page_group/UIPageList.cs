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
    internal sealed class UIPageList
    {
        private LinkedList<CPtr<IUIGroupPage>> _list = new LinkedList<CPtr<IUIGroupPage>>();

        public int Count => _list.Count;

        public void AddFirst(IUIGroupPage page)
        {
            if (page == null)
                return;
            _list.ExtAddFirst(new CPtr<IUIGroupPage>(page));
        }

        public void AddLast(IUIGroupPage page)
        {
            if (page == null)
                return;
            _list.ExtAddLast(new CPtr<IUIGroupPage>(page));
        }

        public void ClearEmpty()
        {
            LinkedListNode<CPtr<IUIGroupPage>> next = null;
            for (var node = _list.First; node != null; node = next)
            {
                next = node.Next;
                IUIGroupPage page = node.Value.Val;
                if (page == null)
                {
                    _list.ExtRemove(node);
                }
            }
        }

        public LinkedListNode<CPtr<IUIGroupPage>> GetFirst()
        {
            LinkedListNode<CPtr<IUIGroupPage>> next = null;
            for (var node = _list.First; node != null; node = next)
            {
                next = node.Next;
                IUIGroupPage page = node.Value.Val;

                if (page == null)
                {
                    _list.ExtRemove(node);
                }
                else
                {
                    return node;
                }
            }
            return null;
        }

        public LinkedListNode<CPtr<IUIGroupPage>> FindFirst<T>() where T : class, IUIGroupPage
        {
            LinkedListNode<CPtr<IUIGroupPage>> next = null;
            for (var node = _list.First; node != null; node = next)
            {
                next = node.Next;
                IUIGroupPage page = node.Value.Val;
                if (page == null)
                {
                    _list.ExtRemove(node);
                }
                else if (page is T)
                {
                    return node;
                }
            }
            return null;
        }

        public LinkedListNode<CPtr<IUIGroupPage>> Find(int pageId)
        {
            LinkedListNode<CPtr<IUIGroupPage>> next = null;
            for (var node = _list.First; node != null; node = next)
            {
                next = node.Next;
                IUIGroupPage page = node.Value.Val;

                if (page == null)
                {
                    _list.ExtRemove(node);
                }
                else if (page.UIElementId == pageId)
                {
                    return node;
                }
            }
            return null;
        }
    }
}
