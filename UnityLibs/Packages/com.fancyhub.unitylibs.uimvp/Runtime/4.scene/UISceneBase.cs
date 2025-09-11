using System;
using System.Collections.Generic;

namespace FH.UI
{
    public abstract class UISceneBase : UIElement, IUIScene
    {
        public abstract void OnSceneEnter(IUIScene lastScene);
        public virtual void OnSceneExit(IUIScene nextScene)
        {
        }

        public abstract void OnUpdate();

        public T OpenUI<T>(
            EUIPageGroupChannel GroupChannel = EUIPageGroupChannel.Free,
            bool GroupUniquePage = false,
            bool AddToScene = true,
            EUIViewLayer ViewLayer = EUIViewLayer.Normal,
            UnityEngine.RectTransform Parent = null,
            EUITagIndex Tag = EUITagIndex.None,
            IResInstHolder ResHolder = null) where T : UIPageBase, new()
        {
            PageOpenInfo defaultInfo = PageOpenInfo.Default;
            defaultInfo.GroupChannel = GroupChannel;
            defaultInfo.GroupUniquePage = GroupUniquePage;
            defaultInfo.AddToScene = AddToScene;
            defaultInfo.ViewLayer = ViewLayer;
            defaultInfo.Tag = Tag;
            defaultInfo.ResHolder = ResHolder;
            defaultInfo.ViewParent = Parent;

            return UIMgr.OpenUI<T>(defaultInfo);
        }
    }
}
