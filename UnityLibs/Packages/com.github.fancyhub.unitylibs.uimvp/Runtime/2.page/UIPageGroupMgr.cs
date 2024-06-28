/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/6/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


namespace FH
{
    public enum EUIPageGroup
    {
        None,
        Stack,
        Queue,
    }

    public struct UIPageGroupInfo
    {
        private readonly int _scene_id;
        public readonly EUIPageGroup Group;
        public readonly UIPageGroupMgr PageGroupMgr;

        public UIPageGroupInfo(UIPageGroupMgr mgr, int scene_id, EUIPageGroup group)
        {
            this._scene_id = scene_id;
            this.PageGroupMgr = mgr;
            this.Group = group;
        }

        public bool HasScene()
        {
            return _scene_id > 0;
        }
    }

    public interface IUIPageWithGroupInfo : IUIPage
    {
        public UIPageGroupInfo UIGroupInfo { get; internal set; }
        public void SetPageGroupVisible(bool visible);
    }

    public sealed class UIPageGroupMgr
    {
        private struct PageInfo
        {
            public int SceneId;
            public EUIPageGroup Group;
            public CPtr<IUIPageWithGroupInfo> Page;
        }

        private int _current_scene_id = 0;
        private MyDict<int, PageInfo> _dict = new MyDict<int, PageInfo>();
        private IUIPageGroup[] _groups = new IUIPageGroup[(int)EUIPageGroup.Queue + 1];

        public UIPageGroupMgr()
        {
            _groups[(int)EUIPageGroup.None] = new UIPageGroupFree();
            _groups[(int)EUIPageGroup.Stack] = new UIPageGroupStack();
            _groups[(int)EUIPageGroup.Queue] = new UIPageGroupQueue();
        }

        /// <summary>
        /// 会删除旧场景的所有Page
        /// </summary>
        public void ChangeScene(int sceneId)
        {
            int old_scene_id = _current_scene_id;
            _current_scene_id = sceneId;

            if (old_scene_id == 0) //0是无场景
                return;

            foreach (var p in _dict)
            {
                var page = p.Value.Page.Val;
                if (page == null)
                {
                    _dict.Remove(p.Key);
                    continue;
                }

                if (p.Value.SceneId != old_scene_id)
                    continue;

                page.Destroy();
                _dict.Remove(p.Key);
            }

            foreach (var p in _groups)
            {
                p.ClearEmpty();
            }
        }

        public bool AddPage(IUIPageWithGroupInfo page, EUIPageGroup group = EUIPageGroup.None, bool add_to_scene = true)
        {
            if (page == null)
                return false;
            if (group <= EUIPageGroup.None || group >= EUIPageGroup.Queue)
                return false;

            int scene_id = _current_scene_id;
            if (!add_to_scene)
                scene_id = 0;

            //已经添加了
            if (_dict.TryGetValue(page.Id, out var info))
                return false;


            _dict.Add(page.Id, new PageInfo()
            {
                SceneId = scene_id,
                Page = new CPtr<IUIPageWithGroupInfo>(page),
                Group = group,
            });

            page.UIGroupInfo = new UIPageGroupInfo(this, scene_id, group);
            _groups[(int)group].AddPage(page);
            return true;
        }

        public bool RemovePage(int pageId)
        {
            if (_dict.Remove(pageId, out var info))
                return false;

            _groups[(int)info.Group].RemovePage(pageId);
            return true;
        }

        public IUIPageWithGroupInfo ShowPage<T>(EUIPageGroup group) where T : class, IUIPageWithGroupInfo
        {
            return _groups[(int)group].ShowPage<T>();
        }
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


    sealed class UIPageGroupFree : IUIPageGroup
    {
        private UIPageList _list = new UIPageList();

        public EUIPageGroup GroupType => EUIPageGroup.None;

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
