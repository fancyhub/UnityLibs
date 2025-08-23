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
    /// <summary>
    /// 带有Group 信息, Res 的管理
    /// </summary>
    public abstract class UIPageBase : UIElement, IUIPage, IUIGroupPage, IUITagPage, IUILayerViewPage, IUIScenePage, IUIResPage
    {
        public UIPageBase() { }

        #region Public Method
        public virtual void UIOpen()
        {
            _ProcessStateMsg(EMsg.Open);
        }

        public virtual void UIShow()
        {
            if (_PageStatusData.PageVisible)
                return;
            _PageStatusData.PageVisible = true;
            UILog._.D("Page:{0},{1},  UIVisible true", UIElementId, GetType());
            _ProcessStateMsg(EMsg.UpdateVisible);
        }

        public virtual void UIHide()
        {
            if (!_PageStatusData.PageVisible)
                return;

            _PageStatusData.PageVisible = false;
            UILog._.D("Page:{0},{1},  UIVisible false", UIElementId, GetType());
            _ProcessStateMsg(EMsg.UpdateVisible);
        }

        public virtual void UIClose()
        {
            Destroy();
        }

        public override void Destroy()
        {
            if (IsDestroyed())
            {
                UILog._.E("Duplicate Destroy {0}", GetType());
                return;
            }
            UILog._.D("Destroy, pageId: {0}, pageType: {1}", UIElementId, GetType());
            base.Destroy();

            _ProcessStateMsg(EMsg.Close);

            PtrList?.Destroy();
            PtrList = null;
            _ChildPages?.Clear();

            _GroupPageInfo.Mgr?.RemovePage(UIElementId);
            _UITagPageInfo.Mgr?.RemoveTag(UIElementId);
        }

        public bool IsPageVisible { get { return _PageState == EPageState.Show; } }

        #endregion

        protected virtual T CreateView<T>(Transform parent = null) where T : UIBaseView, new()
        {
            if (parent == null)
            {
                parent = LayerViewPageInfo.GetParent();
                if (parent == null)
                    parent = UIRoot.Root2D;
            }

            T ret = UIBaseView.CreateView<T>(parent, ResHolder);
            PtrList += ret;
            return ret;
        }

        protected abstract void OnUI1PrepareRes(IResInstHolder holder);
        protected abstract void OnUI2Open();
        protected abstract void OnUI3Show();
        protected abstract void OnUI4Hide();
        protected abstract void OnUI5Close();


        #region Child Page
        private List<CPtr<UIPageBase>> _ChildPages;
        public T OpenChildPage<T>(PageOpenInfo pageOpenInfo) where T : UIPageBase, new()
        {
            T ret = new T();

            GroupPageInfo.Mgr?.AddPage(ret, pageOpenInfo.GroupChannel);
            TagPageInfo.Mgr?.AddTag(ret, pageOpenInfo.Tag);
            UIScenePageInfo.Mgr?.AddPage(ret, pageOpenInfo.AddToScene);
            LayerViewPageInfo.Mgr?.AddPage(ret, pageOpenInfo.ViewLayer, pageOpenInfo.ViewParent);
            ((IUIResPage)ret).SetResHolder(_Holder.Val);

            PtrList += ret;
            if (_ChildPages == null)
                _ChildPages = new List<CPtr<UIPageBase>>();
            _ChildPages.Add(ret);

            ret.OnParentPageVisible(this.IsPageVisible);
            ret.UIOpen();
            return ret;
        }

        private void OnParentPageVisible(bool visible)
        {
            if (_PageStatusData.ParentPageVisible == visible)
                return;
            _PageStatusData.ParentPageVisible = visible;
            UILog._.D("Page:{0},{1},  OnParentPageVisible {2}", UIElementId, GetType(), visible);
            _ProcessStateMsg(EMsg.UpdateVisible);
        }

        #endregion

        #region UIResPage
        private CPtr<IResInstHolder> _Holder;
        public IResInstHolder ResHolder
        {
            get
            {
                var ret = _Holder.Val;
                if (ret == null)
                {
                    _Holder = new CPtr<IResInstHolder>(CreateHolder());
                    ret = _Holder.Val;
                    PtrList += ret;
                }
                return ret;
            }
        }
        protected virtual IResInstHolder CreateHolder()
        {
            return ResMgr.CreateHolder(false, true);
        }

        void IHolderCallBack.OnHolderCallBack()
        {
            var holder = _Holder.Val;
            var stat = holder.GetStat();
            if (!stat.IsAllDone)
                return;
            holder.SetCallBack(null);

            if (_PageStatusData.ResState != EResState.Preparing)//严重错误
            {
                UILog._.E("Error");
                return;
            }
            _ProcessStateMsg(EMsg.ResDone);
        }

        void IUIResPage.SetResHolder(IResInstHolder resHolder)
        {
            if (_PageStatusData.ResState != EResState.None)
            {
                UILog._.E("cant set res holder, because res state is {0}", _PageStatusData.ResState);
                return;
            }
            if (resHolder == null || !_Holder.Null)
                return;

            _Holder = new CPtr<IResInstHolder>(resHolder);
            _PageStatusData.ResState = EResState.Done;
        }
        #endregion

        #region IUITagPage
        private UITagPageInfo _UITagPageInfo;
        public UITagPageInfo TagPageInfo { get => _UITagPageInfo; }
        void IUITagPage.SetTagPageInfo(UITagPageInfo info) { _UITagPageInfo = info; }
        void IUITagPage.SetPageTagVisible(bool visible)
        {
            if (_PageStatusData.PageTagVisible == visible)
                return;
            _PageStatusData.PageTagVisible = visible;
            UILog._.D("Page:{0},{1},  SetPageTagVisible {2}", UIElementId, GetType(), visible);
            _ProcessStateMsg(EMsg.UpdateVisible);
        }

        #endregion

        #region IUIGroupPage
        private UIGroupPageInfo _GroupPageInfo;
        public UIGroupPageInfo GroupPageInfo { get => _GroupPageInfo; }
        void IUIGroupPage.SetGroupPageInfo(UIGroupPageInfo info) { _GroupPageInfo = info; }

        void IUIGroupPage.SetPageGroupVisible(bool visible)
        {
            if (_PageStatusData.PageGroupVisible == visible)
                return;
            _PageStatusData.PageGroupVisible = visible;
            UILog._.D("Page:{0},{1},  SetPageGroupVisible {2}", UIElementId, GetType(), visible);
            _ProcessStateMsg(EMsg.UpdateVisible);
        }
        #endregion

        #region IUILayerViewPage
        private UILayerViewPageInfo _UILayerViewPageInfo;
        public UILayerViewPageInfo LayerViewPageInfo { get => _UILayerViewPageInfo; }
        void IUILayerViewPage.SetLayerViewPageInfo(UILayerViewPageInfo info) { _UILayerViewPageInfo = info; }
        #endregion

        #region IUIScenePage
        private UIScenePageInfo _UIScenePageInfo;
        public UIScenePageInfo UIScenePageInfo => _UIScenePageInfo;
        void IUIScenePage.SetUIScenePageInfo(UIScenePageInfo info) { _UIScenePageInfo = info; }
        #endregion

        #region Status

        #region status data
        private enum EResState
        {
            None,
            Preparing,
            Done,
        }

        private enum EPageState
        {
            None,
            PreparingRes,
            ResDone,
            Show,
            Hide,
            Closed,
        }

        private enum EMsg
        {
            None,
            Open,
            ResDone,
            UpdateVisible,
            Close,
        }


        private EPageState _PageState = EPageState.None;

        private struct PageStatusData
        {
            public static PageStatusData Default = new PageStatusData()
            {
                ResState = EResState.None,
                PageTagVisible = true,
                PageGroupVisible = true,
                PageVisible = true,
                ParentPageVisible = true,
            };
            public EResState ResState;
            public bool PageTagVisible;
            public bool PageGroupVisible;
            public bool PageVisible;
            public bool ParentPageVisible;

            public bool CalcVisible()
            {
                return PageTagVisible && PageGroupVisible && PageVisible && ParentPageVisible;
            }
        }

        private PageStatusData _PageStatusData = PageStatusData.Default;

        protected PtrList PtrList;
        #endregion

        private static Action<UIPageBase, EMsg>[] _StateActions = new Action<UIPageBase, EMsg>[]
        {
            _ProcessStateNone,
            _ProcessStatePreparingRes,
            _ProcessStateResDone,
            _ProcessStateShow,
            _ProcessStateHide,
            _ProcessStateClosed,
        };

        private void _ProcessStateMsg(EMsg msg)
        {
            if (_PageState < 0 || (int)_PageState >= _StateActions.Length)
            {
                UILog._.E("unkown state {0}", _PageState);
                return;
            }

            var action = _StateActions[(int)_PageState];
            action(this, msg);
        }

        #region sub status process
        private static void _ProcessStateNone(UIPageBase page, EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, page._PageState);
                    break;

                case EMsg.UpdateVisible:
                    //Do nothing
                    return;

                case EMsg.Close:
                    page._PageState = EPageState.Closed;
                    break;

                case EMsg.Open:
                    switch (page._PageStatusData.ResState)
                    {
                        default:
                            UILog._.E("未处理资源状态 {0} in state {1} with msg {2}", page._PageStatusData.ResState, page._PageState, msg);
                            break;

                        case EResState.None:
                            page._PageState = EPageState.PreparingRes;
                            page._PageStatusData.ResState = EResState.Preparing;
                            if (_CallOnPrepareRes(page))
                                break;

                            page._PageStatusData.ResState = EResState.Done;
                            page._PageState = EPageState.ResDone;

                            if (page._PageStatusData.CalcVisible())
                            {
                                page._PageState = EPageState.Show;
                                _CallOnOpen(page);
                                _CallOnShow(page);
                            }

                            break;

                        case EResState.Done:
                            page._PageStatusData.ResState = EResState.Done;
                            page._PageState = EPageState.ResDone;

                            if (page._PageStatusData.CalcVisible())
                            {
                                page._PageState = EPageState.Show;
                                _CallOnOpen(page);
                                _CallOnShow(page);
                            }
                            break;
                    }
                    break;
            }
        }

        private static void _ProcessStatePreparingRes(UIPageBase page, EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, page._PageState);
                    break;

                case EMsg.UpdateVisible:
                    //Do nothing
                    return;

                case EMsg.Close:
                    page._PageState = EPageState.Closed;
                    break;

                case EMsg.ResDone:

                    switch (page._PageStatusData.ResState)
                    {
                        default:
                            UILog._.E("未处理资源状态 {0} in state {1} with msg {2}", page._PageStatusData.ResState, page._PageState, msg);
                            break;

                        case EResState.Preparing:
                            page._PageState = EPageState.ResDone;
                            page._PageStatusData.ResState = EResState.Done;

                            if (page._PageStatusData.CalcVisible())
                            {
                                page._PageState = EPageState.Show;
                                _CallOnOpen(page);
                                _CallOnShow(page);
                            }
                            break;
                    }
                    break;
            }
        }

        private static void _ProcessStateResDone(UIPageBase page, EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, page._PageState);
                    break;

                case EMsg.Close:
                    page._PageState = EPageState.Closed;
                    break;

                case EMsg.UpdateVisible:
                    if (page._PageStatusData.CalcVisible())
                    {
                        page._PageState = EPageState.Show;
                        _CallOnOpen(page);
                        _CallOnShow(page);
                    }
                    break;
            }
        }

        private static void _ProcessStateShow(UIPageBase page, EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, page._PageState);
                    break;


                case EMsg.Close:
                    {
                        page._PageState = EPageState.Closed;
                        _CallOnHide(page);
                        _CallOnClose(page);
                    }
                    break;

                case EMsg.UpdateVisible:
                    if (!page._PageStatusData.CalcVisible())
                    {
                        page._PageState = EPageState.Hide;
                        _CallOnHide(page);
                    }
                    break;
            }
        }

        private static void _ProcessStateHide(UIPageBase page, EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, page._PageState);
                    break;

                case EMsg.Close:
                    {
                        page._PageState = EPageState.Closed;
                        _CallOnClose(page);
                    }
                    break;

                case EMsg.UpdateVisible:
                    if (page._PageStatusData.CalcVisible())
                    {
                        page._PageState = EPageState.Show;
                        _CallOnShow(page);
                    }
                    break;
            }
        }

        private static void _ProcessStateClosed(UIPageBase page, EMsg msg)
        {
            UILog._.E("Page:{0},{1} is closed", page.UIElementId, page.GetType());
        }
        #endregion

        #region call on callback
        private static bool _CallOnPrepareRes(UIPageBase page)
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIPrepareRes", page.UIElementId, page.GetType());
                var holder = page.ResHolder;
                page.OnUI1PrepareRes(holder);

                if (!holder.GetStat().IsAllDone) //如果没有需要的,直接实例化
                {
                    holder.SetCallBack(page);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                UILog._.E(ex);
                return false;
            }
        }

        private static void _CallOnOpen(UIPageBase page)
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIOpen", page.UIElementId, page.GetType());
                page.OnUI2Open();
            }
            catch (Exception ex)
            {
                UILog._.E(ex);
            }
        }

        private static void _CallOnShow(UIPageBase page)
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIShow", page.UIElementId, page.GetType());
                page.OnUI3Show();
                page.TagPageInfo.Mgr?.ApplyMask(page, page.TagPageInfo.TagIndex);

                if (page._ChildPages != null)
                    foreach (var p in page._ChildPages)
                        p.Val?.OnParentPageVisible(true);

            }
            catch (Exception ex)
            {
                UILog._.E(ex);
            }
        }

        private static void _CallOnHide(UIPageBase page)
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIHide", page.UIElementId, page.GetType());
                page.TagPageInfo.Mgr?.WithdrawMask(page.UIElementId);
                page.OnUI4Hide();

                if (page._ChildPages != null)
                    foreach (var p in page._ChildPages)
                        p.Val?.OnParentPageVisible(false);
            }
            catch (Exception ex)
            {
                UILog._.E(ex);
            }
        }

        private static void _CallOnClose(UIPageBase page)
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIClose", page.UIElementId, page.GetType());
                page.OnUI5Close();
            }
            catch (Exception ex)
            {
                UILog._.E(ex);
            }
        }
        #endregion
        #endregion

    }

    public abstract class UIPageBase<TView> : UIPageBase where TView : UIBaseView, new()
    {
        public TView BaseView;

        protected override void OnUI1PrepareRes(IResInstHolder holder) { }

        protected override void OnUI2Open()
        {
            BaseView = CreateView<TView>();
        }

        protected override void OnUI3Show()
        {
            if (BaseView != null)
                BaseView.Active = true;
        }

        protected override void OnUI4Hide()
        {
            if (BaseView != null)
                BaseView.Active = false;
        }

        protected override void OnUI5Close()
        {
        }
    }
}
