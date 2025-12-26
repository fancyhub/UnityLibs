using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using System;
using System.Globalization;

namespace Game
{
    public class UITestWebViewPage : UIPageBase<UITestWebViewView>
    {
        public class MyTab
        {
            public UIWebViewTabView Tab;
            public UIWebViewView View;
            public UIWebViewUrlView Url;

            public void Init()
            {
                _OnTabChanged(Tab._WebViewTab.isOn);
                this.Url._WebViewUrl.onSubmit.AddListener(_OnUrlSubmit);
                this.Tab._WebViewTab.onValueChanged.AddListener(_OnTabChanged);
            }

            public void Close()
            {
                Tab._WebViewTab.group = null;
                View.Destroy();
                Tab.Destroy();
            }

            private void _OnUrlSubmit(string url)
            {
                View.Open(url);
            }

            private void _OnTabChanged(bool v)
            {
                View.Active = v;
                Url.Active = v;
            }
        }

        public List<MyTab> _AllViews = new List<MyTab>();
        public int _CurrentIndex = 0;
        protected override void OnUI2Open()
        {
            if (!WebViewMgr.HasSetEnv())
            {
                WebViewMgr.SetEnv(new WebViewEnv()
                {
                    JavascriptHostObjName = "FH",
                });
            }

            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            _CurrentIndex = -1;
            BaseView._BtnAddTab.OnClick = _OnBtnAddClick;
        }


        private void _OnBtnAddClick()
        {
            MyTab tab = new MyTab();
            _AllViews.Add(tab);

            tab.Tab = UIBaseView.CreateView<UIWebViewTabView>(BaseView._Tabs.transform, ResHolder);
            tab.View = UIBaseView.CreateView<UIWebViewView>(BaseView._WebViewDummy.rectTransform, ResHolder);
            tab.Url = UIBaseView.CreateView<UIWebViewUrlView>(BaseView._Url.transform, ResHolder);
            BaseView._BtnAddTab.SelfRoot.transform.SetAsLastSibling();

            tab.Init();
            tab.Tab._WebViewTab.group = BaseView._Tabs;
            tab.Tab._WebViewTab.isOn = true;
            tab.Tab._Close.OnClick2 = _OnBtnCloseTabClick;
        }

        private void _OnBtnCloseTabClick(UIButtonView btn)
        {
            int index = -1;
            for (int i = 0; i < _AllViews.Count; i++)
            {
                if (_AllViews[i].Tab._Close == btn)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
                return;

            _AllViews[index].Close();
            _AllViews.RemoveAt(index);
        }

        private void _OnBtnCloseClick()
        {
            this.UIClose();
            foreach (var p in _AllViews)
            {
                p.Close();
            }
            _AllViews.Clear();
        }
    }
}