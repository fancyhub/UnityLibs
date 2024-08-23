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

        public EUIPageGroup GroupType => EUIPageGroup.Free;

        public void ClearEmpty()
        {
            _list.ClearEmpty();
        }

        public void AddPage(IUIPageWithGroupInfo page)
        {
            if (page == null)
                return;
            _list.AddLast(page);
            page.SetPageGroupVisible(true);
        }

        public void RemovePage(int pageId)
        {
            var node = _list.Find(pageId);
            if (node == null)
                return;
            node.ExtRemoveFromList();
        }

        public IUIPageWithGroupInfo ShowPage<T>() where T : class, IUIPageWithGroupInfo
        {
            var node = _list.FindFirst<T>();
            if (node == null)
                return null;

            node.Value.Val.SetPageGroupVisible(true);
            return node.Value.Val as T;
        }
    }
}
