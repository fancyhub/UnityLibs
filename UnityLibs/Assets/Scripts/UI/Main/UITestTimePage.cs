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
        private List<CultureInfo> _AllCultures = new List<CultureInfo>();
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            UIMgr.UpdateList += this;


            _AllCultures.Clear();
            _AllCultures.AddRange(System.Globalization.CultureInfo.GetCultures(CultureTypes.AllCultures));

            _AllCultures.Sort((a, b) =>
            {
                return a.Name.CompareTo(b.Name);
            });

            List<string> all_options = new List<string>(_AllCultures.Count);
            string current_name = System.Globalization.CultureInfo.CurrentCulture.Name;
            int index = -1;
            for (int i = 0; i < _AllCultures.Count; i++)
            {
                string name = _AllCultures[i].Name;
                all_options.Add(name);
                if (name == current_name)
                {
                    index = i;
                }
            }

            BaseView._DropDownCulture.options.Clear();
            BaseView._DropDownCulture.AddOptions(all_options);

            if (index >= 0)
                BaseView._DropDownCulture.value = index;

            BaseView._DropDownCulture.onValueChanged.AddListener(_OnDropwDownChanged);
        }

        protected override void OnUI5Close()
        {
            BaseView._DropDownCulture.onValueChanged.RemoveAllListeners();
            base.OnUI5Close();
        }

        private void _OnDropwDownChanged(int index)
        {
            System.Globalization.CultureInfo.CurrentCulture = _AllCultures[index];
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
CultureInfo.CurrentCulture: {System.Globalization.CultureInfo.CurrentCulture}

CurrentThread.CurrentUICulture: {System.Threading.Thread.CurrentThread.CurrentUICulture}
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