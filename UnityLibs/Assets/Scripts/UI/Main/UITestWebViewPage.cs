using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using System;
using System.Globalization;

namespace Game
{
    public class UITestWebViewPage: UIPageBase<UITestWebViewView>
    {
        protected override void OnUI2Open()
        {
            if (!UnityWebView.HasSetEnv())
            {
                UnityWebView.SetEnv(new WebViewEnv()
                {
                    JavascriptHostObjName = "FH",
                });
            }

            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;           
        }         

        private void _OnBtnCloseClick()
        {
            this.UIClose();
        }
    }
}