using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using System;
using System.Globalization;
using System.Reflection;

namespace Game
{
    public class UITestWebViewPage : UIPageBase<UITestWebViewView>, IUIUpdater
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
                Url.Destroy();
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

            public bool IsActive()
            {
                if (View == null)
                    return false;
                return View.Active;
            }

            public void Update()
            {
                string t = View.UnityWebView.GetTitle();
                if (!string.IsNullOrEmpty(t))
                    this.Tab._Name.text = t;
                else
                    this.Tab._Name.text = "";
            }
        }

        public List<MyTab> _AllViews = new List<MyTab>();        
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            BaseView._BtnAddTab.OnClick = _OnBtnAddClick;

            UIMgr.UpdateList += this;
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

        public void OnUIUpdate(float dt)
        {
            MyTab tab = null;
            foreach (var p in _AllViews)
            {
                if (p!=null && p.IsActive())
                {
                    tab = p;
                    break;
                }
            }            
            
            if (tab == null) 
                return;

            tab.Update();
        }
    }
}