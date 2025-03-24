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
    sealed class UIPageGroupFree : IUIPageGroup
    {
        private UIPageList _list = new UIPageList();

        public EUIPageGroupType GroupType => EUIPageGroupType.Free;
        public EUIPageGroupChannel Channel { get; internal set; }
      
        public UIPageGroupFree(EUIPageGroupChannel channel)
        {
            Channel = channel;
        }

        public void ClearEmpty()
        {
            _list.ClearEmpty();
        }

        public bool AddPage(IUIGroupPage page)
        {
            if (page == null)
                return false;
            _list.AddLast(page);
            page.SetPageGroupVisible(true);
            return true;
        }

        public bool RemovePage(int pageId)
        {
            var node = _list.Find(pageId);
            if (node == null)
            {                
                return false;
            }
            node.ExtRemoveFromList();
            return true;
        }

        public T ShowPage<T>() where T : class, IUIGroupPage
        {
            var node = _list.FindFirst<T>();
            if (node == null)
                return null;

            node.Value.Val.SetPageGroupVisible(true);
            return node.Value.Val as T;
        }
    }
}
