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
    public sealed class UIPageGroupMgr : IUIPageGroupMgr
    {
        private struct PageInfo
        {
            public EUIPageGroupChannel Channel;
            public EUIPageGroupType GroupType;
            public CPtr<IUIGroupPage> Page;
        }

        private MyDict<int, PageInfo> _dict = new MyDict<int, PageInfo>();
        private IUIPageGroup[] _groups;

        public UIPageGroupMgr(params IUIPageGroup[] groups)
        {
            if (groups == null || groups.Length == 0)
            {
                _groups = new IUIPageGroup[0];
                return;
            }

            int maxCount = (int)groups[0].Channel;
            foreach (var p in groups)
            {
                maxCount = Math.Max(maxCount, (int)p.Channel);
            }

            _groups = new IUIPageGroup[maxCount + 1];
            foreach (var p in groups)
            {
                UILog._.Assert(_groups[(int)p.Channel] == null, "Channel {0}, duplicate", p.Channel);
                _groups[(int)p.Channel] = p;
            }
        }

        public UIPageGroupMgr()
            : this(
                  new UIPageGroupFree(EUIPageGroupChannel.Free),
                  new UIPageGroupStack(EUIPageGroupChannel.Stack),
                  new UIPageGroupQueue(EUIPageGroupChannel.Queue))
        {
        }

        public IUIPageGroup GetGroup(EUIPageGroupChannel channel)
        {
            if (channel < 0 || (int)channel >= _groups.Length)
                return null;

            return _groups[(int)channel];
        }

        public bool AddPage(IUIGroupPage page, EUIPageGroupChannel channel)
        {
            if (page == null)
                return false;
            page.SetGroupPageInfo(new UIGroupPageInfo(this));
            var group = GetGroup(channel);
            if (group == null)
                return false;

            //已经添加了
            if (_dict.TryGetValue(page.UIElementId, out var info))
            {
                UILog._.W("Page:{0},{1}, add page to group duplicate", page.UIElementId, page.GetType());
                return false;
            }

            _dict.Add(page.UIElementId, new PageInfo()
            {
                Page = new CPtr<IUIGroupPage>(page),
                GroupType = group.GroupType,
                Channel = channel,
            });
            page.SetGroupPageInfo(new UIGroupPageInfo(this, channel));
            group.AddPage(page);
            UILog._.D("Page:{0},{1},add page to group succ", page.UIElementId, page.GetType(), group);
            return true;
        }

        public bool RemovePage(int pageId)
        {
            if (!_dict.Remove(pageId, out var info))
            {
                UILog._.D("Page:{0}, remove page from group failed, Cant find page", pageId);
                return false;
            }

            var group = GetGroup(info.Channel);
            if (group == null)
            {
                UILog._.E("Page:{0}, remove page from group failed, canot find group {1}", pageId, info.Channel);
                return false;
            }
            group.RemovePage(pageId);
            UILog._.D("Page:{0}, remove page from group succ {1}", pageId, info.Channel);
            return true;
        }

        public T ShowPage<T>(EUIPageGroupChannel channel) where T : class, IUIGroupPage
        {
            var group = GetGroup(channel);
            if (group == null)
                return null;
            return group.ShowPage<T>();
        }
    }
}
