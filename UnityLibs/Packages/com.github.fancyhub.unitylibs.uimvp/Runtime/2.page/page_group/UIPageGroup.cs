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
    public enum EUIPageGroup
    {
        Free,
        Stack,
        Queue,
    }

    public struct UIPageGroupInfo
    {
        public readonly int SceneId;
        public readonly EUIPageGroup Group;
        public readonly IUIPageGroupMgr Mgr;

        public UIPageGroupInfo(IUIPageGroupMgr mgr, int scene_id, EUIPageGroup group)
        {
            this.SceneId = scene_id;
            this.Mgr = mgr;
            this.Group = group;
        }

        public bool HasScene()
        {
            return SceneId > 0;
        }
    }

    public interface IUIPageWithGroupInfo : IUIPage
    {
        public UIPageGroupInfo UIGroupInfo { get; internal set; }
        public void SetPageGroupVisible(bool visible);
    }


    /// <summary>
    /// 这里只是管理 Page, 调用的方法就是 Show 和 Hide
    /// </summary>
    public interface IUIPageGroup
    {
        /// <summary>
        /// clear 空的UIPage
        /// </summary>
        public void ClearEmpty();
        public void AddPage(IUIPageWithGroupInfo page);
        public void RemovePage(int pageId);
        public IUIPageWithGroupInfo ShowPage<T>() where T : class, IUIPageWithGroupInfo;

        public EUIPageGroup GroupType { get; }
    }

    /// <summary>
    /// PageGroup 的管理类
    /// </summary>
    public interface IUIPageGroupMgr
    {
        public bool AddPage(IUIPageWithGroupInfo page, EUIPageGroup group = EUIPageGroup.Free, bool add_to_scene = true);
        public bool RemovePage(int pageId);
        public IUIPageWithGroupInfo ShowPage<T>(EUIPageGroup group) where T : class, IUIPageWithGroupInfo;
        public IUIPageGroup GetGroup(EUIPageGroup group);
    }   

    sealed class UIPageList
    {
        private LinkedList<CPtr<IUIPageWithGroupInfo>> _list = new LinkedList<CPtr<IUIPageWithGroupInfo>>();

        public int Count => _list.Count;

        public void AddFirst(IUIPageWithGroupInfo page)
        {
            if (page == null)
                return;
            _list.ExtAddFirst(new CPtr<IUIPageWithGroupInfo>(page));
        }

        public void AddLast(IUIPageWithGroupInfo page)
        {
            if (page == null)
                return;
            _list.ExtAddLast(new CPtr<IUIPageWithGroupInfo>(page));
        }

        public void ClearEmpty()
        {
            LinkedListNode<CPtr<IUIPageWithGroupInfo>> next = null;
            for (var node = _list.First; node != null; node = next)
            {
                next = node.Next;
                IUIPage page = node.Value.Val;
                if (page == null)
                {
                    _list.ExtRemove(node);
                }
            }
        }

        public LinkedListNode<CPtr<IUIPageWithGroupInfo>> GetFirst()
        {
            LinkedListNode<CPtr<IUIPageWithGroupInfo>> next = null;
            for (var node = _list.First; node != null; node = next)
            {
                next = node.Next;
                IUIPage page = node.Value.Val;

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

        public LinkedListNode<CPtr<IUIPageWithGroupInfo>> FindFirst<T>() where T : class, IUIPageWithGroupInfo
        {
            LinkedListNode<CPtr<IUIPageWithGroupInfo>> next = null;
            for (var node = _list.First; node != null; node = next)
            {
                next = node.Next;
                IUIPage page = node.Value.Val;
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

        public LinkedListNode<CPtr<IUIPageWithGroupInfo>> Find(int pageId)
        {
            LinkedListNode<CPtr<IUIPageWithGroupInfo>> next = null;
            for (var node = _list.First; node != null; node = next)
            {
                next = node.Next;
                IUIPage page = node.Value.Val;

                if (page == null)
                {
                    _list.ExtRemove(node);
                }
                else if (page.Id == pageId)
                {
                    return node;
                }
            }
            return null;
        }
    }
}
