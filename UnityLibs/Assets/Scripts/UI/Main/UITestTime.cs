using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using System;

namespace Game
{
    public class UITestTime : UIPageBase<UITestTimeView>, IUIUpdater
    {
        private static DateTime _start_time = DateUtil.NowLocal.DateTime;
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            UISceneMgr.AddUpdate(this);
        }

        void IUIUpdater.OnUIUpdate()
        {
            BaseView._CurInfo.text = @$"
{DateUtil.NowLocal.DateTime-_start_time} 
{DateUtil.NowLocal.DateTime}
{DateTime.Now}
{DateUtil.NowLocal.DateTime - DateTime.Now}";
        }

        private void _OnBtnCloseClick()
        {
             
            this.UIClose();
        }         
    }
}