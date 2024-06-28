/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH
{
    /// <summary>
    /// 带有Group 信息, Res 的管理
    /// </summary>
    public abstract class UIAbstractPage : UIElement, IUIPageWithGroupInfo, IUIPageTag, IHolderCallBack
    {
        public PtrList PtrList;

        private enum EResState
        {
            None,
            Preparing, //准备中
            Prepared, //准备好了
            Inited, //初始化好了
            Destroyed, //关闭了
        }

        protected IResInstHolder _holder;
        private EResState _res_state = EResState.None;
        protected bool _page_tag_visible = true;
        protected bool _page_group_visible = true;
        private UIPageGroupInfo _page_group_info;

        UIPageGroupInfo IUIPageWithGroupInfo.UIGroupInfo { get => _page_group_info; set => _page_group_info = value; }

        public UIAbstractPage() { }
        public UIAbstractPage(IResInstHolder holder)
        {
            _holder = holder;
            if (_holder != null)
            {
                _res_state = EResState.Prepared;
            }
        }

        protected virtual IResInstHolder CreateHolder()
        {
            var ret = ResMgr.CreateHolder(true, true);
            PtrList += ret;
            return ret;
        }

        public virtual void OpenPage()
        {
            _UpdateUIPageVisible(CalcPageVisible());
        }

        protected virtual bool CalcPageVisible()
        {
            if (_page_group_visible && _page_tag_visible)
                return true;
            return false;
        }

        public virtual void ClosePage()
        {
            Destroy();
        }

        protected virtual void OnUIPrepareRes(IResInstHolder holder) { }
        protected abstract void OnUIInit();
        protected abstract void OnUIShow();
        protected abstract void OnUIHide();
        protected abstract void OnUIClose();

        public override void Destroy()
        {
            if (IsDestroyed())
                return;
            base.Destroy();

            _res_state = EResState.Destroyed;
            OnUIClose();

            PtrList?.Destroy();
            PtrList = null;

            _page_group_info.PageGroupMgr?.RemovePage(Id);
        }

        void IUIPageWithGroupInfo.SetPageGroupVisible(bool visible)
        {
            if (_page_group_visible == visible)
                return;

            var old_visible = CalcPageVisible();
            _page_group_visible = visible;
            var new_visible = CalcPageVisible();

            if (new_visible != old_visible)
            {
                _UpdateUIPageVisible(new_visible);
            }
        }

        void IUIPageTag.SetPageTagVisible(bool visible)
        {
            if (_page_tag_visible == visible)
                return;
            var old_visible = CalcPageVisible();
            _page_tag_visible = visible;
            var new_visible = CalcPageVisible();

            if (new_visible != old_visible)
            {
                _UpdateUIPageVisible(new_visible);
            }
        }
        void IHolderCallBack.OnHolderCallBack()
        {
            if (!_holder.GetStat().IsAllDone)
                return;
            _holder.SetCallBack(null);

            if (_res_state != EResState.Preparing)//严重错误
                return;
            _res_state = EResState.Prepared;

            var visible = CalcPageVisible();
            _UpdateUIPageVisible(visible);
        }


        protected void _UpdateUIPageVisible(bool v)
        {
            if (!v)
            {
                if (_res_state == EResState.Inited)
                {
                    OnUIHide();
                }
                return;
            }


            switch (_res_state)
            {
                case EResState.None:
                    _holder = CreateHolder();

                    //收集需要的资源
                    _res_state = EResState.Preparing;
                    OnUIPrepareRes(_holder);

                    if (_holder.GetStat().IsAllDone) //如果没有需要的,直接实例化
                    {
                        _holder.SetCallBack(this);
                        return;
                    }

                    _res_state = EResState.Inited;
                    OnUIInit();
                    OnUIShow();
                    break;

                case EResState.Preparing:
                    break;

                case EResState.Prepared:
                    _res_state = EResState.Inited;
                    OnUIInit();
                    OnUIShow();
                    break;

                case EResState.Inited:
                    OnUIShow();
                    break;

                case EResState.Destroyed:
                    return;
            }
        }
    }
}
