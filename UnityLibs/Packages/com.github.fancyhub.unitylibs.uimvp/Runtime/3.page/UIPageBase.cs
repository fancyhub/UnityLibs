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
        internal enum EResState
        {
            None,
            Preparing,
            Done,
        }

        internal enum EPageState
        {
            None,
            PreparingRes,
            ResDone,
            Show,
            Hide,
            Closed,
        }

        internal enum EMsg
        {
            None,
            Open,
            ResDone,
            UpdateVisible,
            Close,
        }

        public IResInstHolder _Holder;
        private EPageState _PageState = EPageState.None;

        internal struct PageStatusData
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

        public PtrList PtrList;
        private List<CPtr<UIPageBase>> _ChildPages;

        public UIPageBase() { }

        public T OpenChildPage<T>(PageOpenInfo pageOpenInfo) where T : UIPageBase, new()
        {
            T ret = new T();

            GroupPageInfo.Mgr?.AddPage(ret, pageOpenInfo.GroupChannel);
            TagPageInfo.Mgr?.AddTag(ret, pageOpenInfo.Tag);
            UIScenePageInfo.Mgr?.AddPage(ret, pageOpenInfo.AddToScene);
            LayerViewPageInfo.Mgr?.AddPage(ret, pageOpenInfo.ViewLayer, pageOpenInfo.ViewParent);
            ((IUIResPage)ret).SetResHolder(_Holder);

            PtrList += ret;
            if (_ChildPages == null)
                _ChildPages = new List<CPtr<UIPageBase>>();
            _ChildPages.Add(ret);

            ret.UIOpen();
            return ret;
        }

        public virtual void UIOpen()
        {
            _ProcessStateMsg(EMsg.Open);
        }

        public virtual void UIShow()
        {
            if (_PageStatusData.PageVisible)
                return;
            _PageStatusData.PageVisible = true;
            UILog._.D("Page:{0},{1},  UIVisible true", Id, GetType());
            _ProcessStateMsg(EMsg.UpdateVisible);
        }

        public virtual void UIHide()
        {
            if (!_PageStatusData.PageVisible)
                return;

            _PageStatusData.PageVisible = false;
            UILog._.D("Page:{0},{1},  UIVisible false", Id, GetType());
            _ProcessStateMsg(EMsg.UpdateVisible);
        }

        public virtual void UIClose()
        {
            Destroy();
        }

        protected virtual void OnParentPageVisible(bool visible)
        {
            if (_PageStatusData.ParentPageVisible == visible)
                return;
            _PageStatusData.ParentPageVisible = visible;
            UILog._.D("Page:{0},{1},  OnParentPageVisible {2}", Id, GetType(), visible);
            _ProcessStateMsg(EMsg.UpdateVisible);
        }

        public virtual T CreateView<T>(Transform parent = null) where T : UIBaseView, new()
        {
            if (parent == null)
            {
                parent = LayerViewPageInfo.GetParent();
                if (parent == null)
                    parent = UIRoot.Root2D;
            }

            T ret = UIBaseView.CreateView<T>(parent, _Holder);
            PtrList += ret;
            return ret;
        }

        public bool IsVisible
        {
            get
            {
                return _PageState == EPageState.Show;
            }
        }

        protected abstract void OnUI1PrepareRes(IResInstHolder holder);
        protected abstract void OnUI2Init();
        protected abstract void OnUI3Show();
        protected abstract void OnUI4Hide();
        protected abstract void OnUI5Close();

        public override void Destroy()
        {
            if (IsDestroyed())
            {
                UILog._.E("Duplicate Destroy {0}", GetType());
                return;
            }
            UILog._.D("Destroy, pageId: {0}, pageType: {1}", Id, GetType());
            base.Destroy();

            _ProcessStateMsg(EMsg.Close);

            PtrList?.Destroy();
            PtrList = null;
            _ChildPages?.Clear();

            _GroupPageInfo.Mgr?.RemovePage(Id);
            _UITagPageInfo.Mgr?.RemoveTag(Id);
        }

        #region UIResPage
        protected virtual IResInstHolder CreateHolder()
        {
            var ret = ResMgr.CreateHolder(false, true);
            PtrList += ret;
            return ret;
        }

        void IHolderCallBack.OnHolderCallBack()
        {
            var stat = _Holder.GetStat();
            if (!stat.IsAllDone)
                return;
            _Holder.SetCallBack(null);

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

            if (resHolder == null)
                return;

            _Holder = resHolder;
            _PageStatusData.ResState = EResState.Done;
        }

        public IResInstHolder ResHolder => _Holder;
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
            UILog._.D("Page:{0},{1},  SetPageTagVisible {2}", Id, GetType(), visible);
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
            UILog._.D("Page:{0},{1},  SetPageGroupVisible {2}", Id, GetType(), visible);
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
        private void _ProcessStateMsg(EMsg msg)
        {
            switch (_PageState)
            {
                default:
                    UILog._.E("unkown state {0}", _PageState);
                    break;

                case EPageState.None:
                    _ProcessStateNone(msg);
                    break;

                case EPageState.PreparingRes:
                    _ProcessStatePreparingRes(msg);
                    break;

                case EPageState.ResDone:
                    _ProcessStateResDone(msg);
                    break;

                case EPageState.Show:
                    _ProcessStateShow(msg);
                    break;

                case EPageState.Hide:
                    _ProcessStateHide(msg);
                    break;

                case EPageState.Closed:
                    UILog._.E("Page:{0} is closed", Id);
                    break;
            }
        }

        private void _ProcessStateNone(EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, _PageState);
                    break;

                case EMsg.UpdateVisible:
                    //Do nothing
                    return;

                case EMsg.Close:
                    _PageState = EPageState.Closed;
                    break;

                case EMsg.Open:
                    switch (_PageStatusData.ResState)
                    {
                        default:
                            UILog._.E("未处理资源状态 {0} in state {1} with msg {2}", _PageStatusData.ResState, _PageState, msg);
                            break;

                        case EResState.None:
                            _PageState = EPageState.PreparingRes;
                            _PageStatusData.ResState = EResState.Preparing;
                            if (_CallOnPrepareRes())
                                break;

                            _PageStatusData.ResState = EResState.Done;
                            _PageState = EPageState.ResDone;

                            if (_PageStatusData.CalcVisible())
                            {
                                _PageState = EPageState.Show;
                                _CallOnInit();
                                _CallOnShow();
                            }

                            break;

                        case EResState.Done:
                            _PageStatusData.ResState = EResState.Done;
                            _PageState = EPageState.ResDone;

                            if (_PageStatusData.CalcVisible())
                            {
                                _PageState = EPageState.Show;
                                _CallOnInit();
                                _CallOnShow();
                            }
                            break;
                    }
                    break;
            }
        }

        private void _ProcessStatePreparingRes(EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, _PageState);
                    break;

                case EMsg.UpdateVisible:
                    //Do nothing
                    return;

                case EMsg.Close:
                    _PageState = EPageState.Closed;
                    break;

                case EMsg.ResDone:

                    switch (_PageStatusData.ResState)
                    {
                        default:
                            UILog._.E("未处理资源状态 {0} in state {1} with msg {2}", _PageStatusData.ResState, _PageState, msg);
                            break;

                        case EResState.Preparing:
                            _PageState = EPageState.ResDone;
                            _PageStatusData.ResState = EResState.Done;

                            if (_PageStatusData.CalcVisible())
                            {
                                _PageState = EPageState.Show;
                                _CallOnInit();
                                _CallOnShow();
                            }
                            break;
                    }
                    break;
            }
        }

        private void _ProcessStateResDone(EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, _PageState);
                    break;

                case EMsg.Close:
                    _PageState = EPageState.Closed;
                    break;

                case EMsg.UpdateVisible:
                    if (_PageStatusData.CalcVisible())
                    {
                        _PageState = EPageState.Show;
                        _CallOnInit();
                        _CallOnShow();
                    }
                    break;
            }
        }

        private void _ProcessStateShow(EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, _PageState);
                    break;


                case EMsg.Close:
                    {
                        _PageState = EPageState.Closed;
                        _CallOnHide();
                        _CallOnClose();
                    }
                    break;

                case EMsg.UpdateVisible:
                    if (!_PageStatusData.CalcVisible())
                    {
                        _PageState = EPageState.Hide;
                        _CallOnHide();
                    }
                    break;
            }
        }

        private void _ProcessStateHide(EMsg msg)
        {
            switch (msg)
            {
                default:
                    UILog._.E("未处理消息 {0} in state {1}", msg, _PageState);
                    break;

                case EMsg.Close:
                    {
                        _PageState = EPageState.Closed;
                        _CallOnClose();
                    }
                    break;

                case EMsg.UpdateVisible:
                    if (_PageStatusData.CalcVisible())
                    {
                        _PageState = EPageState.Show;
                        _CallOnShow();
                    }
                    break;
            }
        }

        private bool _CallOnPrepareRes()
        {
            UILog._.D("Page:{0},{1} , OnUIPrepareRes", Id, GetType());
            _Holder = CreateHolder();
            OnUI1PrepareRes(_Holder);


            if (!_Holder.GetStat().IsAllDone) //如果没有需要的,直接实例化
            {
                _Holder.SetCallBack(this);
                return true;
            }
            return false;
        }

        private void _CallOnInit()
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIInit", Id, GetType());
                OnUI2Init();
            }
            catch (Exception ex)
            {
                UILog._.E(ex);
            }
        }

        private void _CallOnShow()
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIShow", Id, GetType());
                OnUI3Show();
                TagPageInfo.Mgr?.ApplyMask(this, TagPageInfo.TagIndex);

                if (_ChildPages != null)
                    foreach (var p in _ChildPages)
                        p.Val?.OnParentPageVisible(true);

            }
            catch (Exception ex)
            {
                UILog._.E(ex);
            }
        }

        private void _CallOnHide()
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIHide", Id, GetType());
                TagPageInfo.Mgr?.WithdrawMask(Id);
                OnUI4Hide();

                if (_ChildPages != null)
                    foreach (var p in _ChildPages)
                        p.Val?.OnParentPageVisible(false);
            }
            catch (Exception ex)
            {
                UILog._.E(ex);
            }
        }

        private void _CallOnClose()
        {
            try
            {
                UILog._.D("Page:{0},{1} , OnUIClose", Id, GetType());
                OnUI5Close();
            }
            catch (Exception ex)
            {
                UILog._.E(ex);
            }
        }
        #endregion

    }

    public abstract class UIPageBase<TView> : UIPageBase where TView : UIBaseView, new()
    {
        public TView BaseView;

        protected override void OnUI1PrepareRes(IResInstHolder holder) { }

        protected override void OnUI2Init()
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
