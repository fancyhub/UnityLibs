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
    public abstract class UIPage : UIElement, IUIPageWithGroupInfo, IUIPageTag, IHolderCallBack, IUILayerViewBGHandler
    {
        private enum EResState
        {
            None,
            Preparing, //准备中
            Prepared, //准备好了
            Inited, //初始化好了
            Destroyed, //关闭了
        }
        private EResState _res_state = EResState.None;
        private UIPageGroupInfo _page_group_info;


        protected IResInstHolder _Holder;
        protected bool _PageTagVisible = true;
        protected bool _PageGroupVisible = true;

        public PtrList PtrList;

        UIPageGroupInfo IUIPageWithGroupInfo.UIGroupInfo { get => _page_group_info; set => _page_group_info = value; }

        public UIPage() { }
        public UIPage(IResInstHolder holder)
        {
            _Holder = holder;
            if (_Holder != null)
            {
                _res_state = EResState.Prepared;
            }
        }

        public virtual void UIOpen()
        {
            _UpdatePageState(CalcPageVisible());
        }

        public virtual void UIClose()
        {
            Destroy();
        }

        public virtual void OnBgClick()
        {
            Log.D("OnBgClick unprocess {0}", this.GetType());
        }

        protected virtual IResInstHolder CreateHolder()
        {
            var ret = ResMgr.CreateHolder(true, true);
            PtrList += ret;
            return ret;
        }

        protected virtual bool CalcPageVisible()
        {
            if (_PageGroupVisible && _PageTagVisible)
                return true;
            return false;
        }

        public T CreateView<T>(Transform parent = null) where T : UIBaseView, new()
        {
            if (parent == null)
                parent = UIRoot.Root2D;
            T ret = UIBaseView.CreateView<T>(parent, _Holder);
            PtrList += ret;
            return ret;
        }

        protected virtual void OnUIPrepareRes(IResInstHolder holder) { }
        protected abstract void OnUIInit();
        protected abstract void OnUIShow();
        protected abstract void OnUIHide();
        protected abstract void OnUIClose();

        public override void Destroy()
        {
            Log.D("OnUIClose");
            if (IsDestroyed())
                return;
            base.Destroy();

            _res_state = EResState.Destroyed;
            try
            {
                OnUIClose();
            }
            catch (Exception ex)
            {
                Log.E(ex);
            }

            PtrList?.Destroy();
            PtrList = null;

            _page_group_info.Mgr?.RemovePage(Id);
        }

        void IUIPageWithGroupInfo.SetPageGroupVisible(bool visible)
        {
            if (_PageGroupVisible == visible)
                return;

            var old_visible = CalcPageVisible();
            _PageGroupVisible = visible;
            var new_visible = CalcPageVisible();

            if (new_visible != old_visible)
            {
                _UpdatePageState(new_visible);
            }
        }

        void IUIPageTag.SetPageTagVisible(bool visible)
        {
            if (_PageTagVisible == visible)
                return;
            var old_visible = CalcPageVisible();
            _PageTagVisible = visible;
            var new_visible = CalcPageVisible();

            if (new_visible != old_visible)
            {
                _UpdatePageState(new_visible);
            }
        }
        void IHolderCallBack.OnHolderCallBack()
        {
            if (!_Holder.GetStat().IsAllDone)
                return;
            _Holder.SetCallBack(null);

            if (_res_state != EResState.Preparing)//严重错误
                return;
            _res_state = EResState.Prepared;

            var visible = CalcPageVisible();
            _UpdatePageState(visible);
        }

        protected void _UpdatePageState(bool visible)
        {
            if (!visible)
            {
                if (_res_state == EResState.Inited)
                {
                    try
                    {
                        OnUIHide();
                    }
                    catch (Exception ex)
                    {
                        Log.E(ex);
                    }
                }
                return;
            }


            switch (_res_state)
            {
                case EResState.None:
                    _Holder = CreateHolder();

                    //收集需要的资源
                    _res_state = EResState.Preparing;
                    OnUIPrepareRes(_Holder);

                    if (!_Holder.GetStat().IsAllDone) //如果没有需要的,直接实例化
                    {
                        _Holder.SetCallBack(this);
                        return;
                    }

                    _res_state = EResState.Inited;
                    try
                    {
                        OnUIInit();
                        OnUIShow();
                    }
                    catch (Exception ex)
                    {
                        Log.E(ex);
                    }

                    break;

                case EResState.Preparing:
                    break;

                case EResState.Prepared:
                    _res_state = EResState.Inited;
                    try
                    {
                        OnUIInit();
                        OnUIShow();
                    }
                    catch (Exception ex)
                    {
                        Log.E(ex);
                    }

                    break;

                case EResState.Inited:
                    try
                    {
                        OnUIShow();
                    }
                    catch (Exception ex)
                    {
                        Log.E(ex);
                    }
                    break;

                case EResState.Destroyed:
                    return;
            }
        }
    }
}
