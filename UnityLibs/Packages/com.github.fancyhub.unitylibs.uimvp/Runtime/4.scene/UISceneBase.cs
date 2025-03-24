using System;
using System.Collections.Generic;

namespace FH.UI
{

    public abstract class UISceneBase : UIElement, IUIScene
    {
        private PtrList _ScenePages;
        protected SceneSharedData _SceneSharedData;

        public UISceneBase()
        {
        }

        SceneSharedData IUIScene.SceneSharedData { get => _SceneSharedData; set => _SceneSharedData = value; }

        public T OpenPage<T>(PageOpenInfo page_open_info)
            where T : class, IUIPage, IUIGroupPage, IUITagPage, IUILayerViewPage, IUIScenePage, new()
        {
            T page = null;
            if (page_open_info.GroupUniquePage)
            {
                page = _SceneSharedData.PageGroupMgr.ShowPage<T>(page_open_info.GroupChannel);
                if (page != null)
                    return page;
            }

            page = new T();
            page.SetUITagPageInfo(new(_SceneSharedData.PageTagMgr));
            page.SetUIGroupPageInfo(new(_SceneSharedData.PageGroupMgr));
            page.SetUILayerViewPageInfo(new(_SceneSharedData.ViewLayerMgr, page_open_info.ViewLayer));
            if (page_open_info.AddToScene)
                _ScenePages += page;
            _SceneSharedData.PageGroupMgr.AddPage(page, page_open_info.GroupChannel);
            page.UIOpen();
            return page;
        }

        public T OpenPage<T>() where T : UIPageBase, new()
        {
            return OpenPage<T>(PageOpenInfo.Default);
        }

        public IUIPageGroupMgr GetPageGroupMgr()
        {
            return _SceneSharedData.PageGroupMgr;
        }

        public virtual void OnSceneEnter(System.Type lastSceneType)
        {
        }

        public virtual void OnSceneExit(System.Type nextSceneType)
        {
            _ScenePages?.Destroy();
            _ScenePages = null;
        }
    }
}
