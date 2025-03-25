/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    public static class UIElementID
    {
        private static int _id_gen = 1;
        public static int Next => _id_gen++;
    }

    public interface IUIElement : IDestroyable, ICPtr
    {
        int Id { get; }
        bool IsDestroyed();
    }

    /// <summary>
    /// UI Presenter
    /// </summary>
    public interface IUIPage : IUIElement
    {
        public void UIOpen();
        public void UIShow();
        public void UIHide();
        public void UIClose();
    }

    /// <summary>
    /// UI View
    /// </summary>
    public interface IUIView : IUIElement
    {
        void SetOrder(int order, bool relative = false);
    }


    #region Res Page
    public interface IUIResPage : IHolderCallBack, IUIElement
    {
        internal void SetResHolder(IResInstHolder resHolder);
        public IResInstHolder ResHolder { get; }
    }
    #endregion

    #region PageGroup
    public enum EUIPageGroupType
    {
        Free,
        Stack,
        Queue,
    }

    public struct UIGroupPageInfo
    {
        public readonly IUIPageGroupMgr Mgr;
        public readonly EUIPageGroupChannel GroupChannel;

        public UIGroupPageInfo(IUIPageGroupMgr mgr, EUIPageGroupChannel channel)
        {
            this.Mgr = mgr;
            this.GroupChannel = channel;
        }

        public UIGroupPageInfo(IUIPageGroupMgr mgr)
        {
            this.Mgr = mgr;
            this.GroupChannel = default;
        }
    }

    public interface IUIGroupPage : IUIElement
    {
        public UIGroupPageInfo GroupPageInfo { get; }
        internal void SetGroupPageInfo(UIGroupPageInfo info);
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
        public bool AddPage(IUIGroupPage page);
        public bool RemovePage(int pageId);
        public T ShowPage<T>() where T : class, IUIGroupPage;

        public EUIPageGroupType GroupType { get; }
        public EUIPageGroupChannel Channel { get; }
    }

    /// <summary>
    /// PageGroup 的管理类
    /// </summary>
    public interface IUIPageGroupMgr
    {
        public bool AddPage(IUIGroupPage page, EUIPageGroupChannel channel);

        public bool RemovePage(int pageId);

        public T ShowPage<T>(EUIPageGroupChannel channel) where T : class, IUIGroupPage;

        public IUIPageGroup GetGroup(EUIPageGroupChannel channel);
    }
    #endregion

    #region Page Tag
    public struct UITagPageInfo
    {
        public readonly IUIPageTagMgr Mgr;
        public UITagPageInfo(IUIPageTagMgr mgr)
        {
            Mgr = mgr;
        }
    }
    public interface IUITagPage : IUIElement
    {
        public UITagPageInfo TagPageInfo { get; }
        internal void SetTagPageInfo(UITagPageInfo info);
        void SetPageTagVisible(bool visible);
    }

    public interface IUIPageTagMgr
    {
        public void AddPage(IUITagPage page, EUITagIndex tagId);
        public void RemovePage(int page_id);
    }
    #endregion

    #region LayerView
    public struct UILayerViewPageInfo
    {
        public readonly IUIViewLayerMgr Mgr;
        public readonly EUIViewLayer ViewLayer;
        public UILayerViewPageInfo(IUIViewLayerMgr mgr, EUIViewLayer viewLayer)
        {
            Mgr = mgr;
            ViewLayer = viewLayer;
        }

        public RectTransform GetNormalLayer()
        {
            if (Mgr == null)
                return null;
            return Mgr.GetLayer(ViewLayer);
        }
    }

    public interface IUILayerView : IUIElement
    {
        public void SetOrder(int order);
        public GameObject GetRoot();
    }

    public interface IUILayerViewPage : IUIElement
    {
        public UILayerViewPageInfo LayerViewPageInfo { get; }
        internal void SetLayerViewPageInfo(UILayerViewPageInfo info);
    }

    public interface IUILayerViewBGHandler : IUIElement
    {
        public void OnBgClick();
    }

    public enum EUIBgClickMode
    {
        None,

        /// <summary>
        /// 普通的模式
        /// </summary>
        Common,

        /// <summary>
        /// 是弹tips的模式, 点击到tip外面都会收到click消息
        /// </summary>
        TipClick,

        /// <summary>
        /// 是弹tips的模式, 点击到tip外面都会收到click消息, 和tip一样,但是down的时候触发
        /// </summary>
        TipDown,
    }

    public interface IUIViewLayerMgr
    {
        public void AddPage(IUILayerViewPage page, EUIViewLayer layer);

        public UnityEngine.RectTransform GetLayer(EUIViewLayer layer);

        public bool AddView(IUILayerView view, int layer_index);
        public bool RemoveView(int view_id);
        public bool MoveView(int view_id, int offset);
        public bool SetBgMask(int view_id, bool enable);
        public bool SetBgClick(int view_id, IUILayerViewBGHandler handler, EUIBgClickMode click_mode);
    }
    #endregion

    #region update
    public interface IUIUpdater : IUIElement
    {
        public void OnUIUpdate();
    }
    public enum EUIUpdateResult
    {
        Continue,
        Stop,
    }

    public delegate EUIUpdateResult ActionUIUpdate();

    public interface IUIUpdaterMgr
    {
        public bool AddUpdate(IUIUpdater updater);
        public int AddUpdate(ActionUIUpdate action);

        public bool RemoveUpdate(int id);

        public void Update();
    }
    #endregion

    #region IUIScene


    public interface IUIScene : IUIElement
    {
        public void OnSceneEnter(IUIScene lastScene);
        public void OnSceneExit(IUIScene nextScene);
    }

    public struct UIScenePageInfo
    {
        public IUISceneMgr Mgr;
    }

    public interface IUIScenePage : IUIElement
    {
        public UIScenePageInfo UIScenePageInfo { get; }
        internal void SetUIScenePageInfo(UIScenePageInfo info);
    }

    public interface IUISceneMgr
    {
        public void AddPage(IUIScenePage page, bool add_to_scene);
    }
    #endregion
}
