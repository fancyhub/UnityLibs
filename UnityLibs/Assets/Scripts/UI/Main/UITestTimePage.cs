using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using System;
using System.Globalization;

namespace Game
{
    public class UITestTimePage : UIPageBase<UITestTimeView>, IUIUpdater
    {
        private static DateTime _start_time = DateUtil.NowLocal.DateTime;
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            UIMgr.UpdateList += this;
        }

        void IUIUpdater.OnUIUpdate(float dt)
        {
            //{ DateUtil.NowLocal.DateTime - _start_time}
            //{ DateUtil.NowLocal.DateTime}
            //{ DateTime.Now}
            //{ DateUtil.NowLocal.DateTime - DateTime.Now}
            BaseView._CurInfo.text = @$"
{DateUtil.NowLocal.DateTime.ToString()}
Android: {GetAndroidCurrentCulture()}
CurrentThread.CurrentCulture: {System.Threading.Thread.CurrentThread.CurrentCulture}
CurrentThread.CurrentUICulture: {System.Threading.Thread.CurrentThread.CurrentUICulture}
CultureInfo.CurrentCulture: {System.Globalization.CultureInfo.CurrentCulture}
CultureInfo.CurrentUICulture: {System.Globalization.CultureInfo.CurrentUICulture}";
        }

        private string _AndroidCode;
        public string GetAndroidCurrentCulture()
        {
            if (_AndroidCode != null) return _AndroidCode;
            try
            {
                // 通过AndroidJavaClass获取系统配置
                using (AndroidJavaClass localeClass = new AndroidJavaClass("java.util.Locale"))
                {
                    AndroidJavaObject defaultLocale = localeClass.CallStatic<AndroidJavaObject>("getDefault");

                    // 获取语言代码（如"zh"、"en"）
                    string language = defaultLocale.Call<string>("getLanguage");
                    // 获取国家/地区代码（如"CN"、"US"）
                    string country = defaultLocale.Call<string>("getCountry");

                    if (!string.IsNullOrEmpty(country))
                    {
                        _AndroidCode = $"{language}-{country}";
                    }
                    else
                    {
                        _AndroidCode = language;
                    }

                    return _AndroidCode;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("获取Android区域设置失败: " + e.Message);
                // 失败时回退到C#的CultureInfo
                _AndroidCode = "";
                return _AndroidCode;
            }
        }

        private void _OnBtnCloseClick()
        {

            this.UIClose();
        }
    }
}