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
            public int SceneId;
            public EUIPageGroup Group;
            public CPtr<IUIPageWithGroupInfo> Page;
        }

        private int _current_scene_id = 0;
        private MyDict<int, PageInfo> _dict = new MyDict<int, PageInfo>();
        private IUIPageGroup[] _groups = new IUIPageGroup[(int)EUIPageGroup.Queue + 1];

        public UIPageGroupMgr()
        {
            _groups[(int)EUIPageGroup.Free] = new UIPageGroupFree();
            _groups[(int)EUIPageGroup.Stack] = new UIPageGroupStack();
            _groups[(int)EUIPageGroup.Queue] = new UIPageGroupQueue();
        }

        public IUIPageGroup GetGroup(EUIPageGroup group)
        {
            if (group < EUIPageGroup.Free || group > EUIPageGroup.Queue)
                return null;
            return _groups[(int)group];
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

        public bool AddPage(IUIPageWithGroupInfo page, EUIPageGroup group = EUIPageGroup.Free, bool add_to_scene = true)
        {
            if (page == null)
                return false;
            if (group <= EUIPageGroup.Free || group >= EUIPageGroup.Queue)
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
}
