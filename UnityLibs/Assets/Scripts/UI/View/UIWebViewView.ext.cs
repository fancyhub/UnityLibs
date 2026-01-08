
using FH;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    public partial class UIWebViewView // : FH.UI.UIBaseView 
    {
        private UIWebView _UnityWebView;
        public override void OnCreate()
        {
            base.OnCreate();

            if (_UnityWebView == null && _WebView!=null)
            {
                _UnityWebView = _WebView.GetComponent<UIWebView>();
                if (_UnityWebView == null)
                    _UnityWebView = _WebView.gameObject.AddComponent<UIWebView>();
            }
        }

        public UIWebView UnityWebView
        {
            get
            {
                if (_UnityWebView != null)
                    return _UnityWebView;

                if (_WebView == null)
                    return null;

                _UnityWebView = _WebView.GetComponent<UIWebView>();
                if (_UnityWebView == null)
                    _UnityWebView = _WebView.gameObject.AddComponent<UIWebView>();
                return _UnityWebView;
            }
        }

        public void Open(string url)
        {
            UnityWebView.Open(url);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_UnityWebView != null)
            {
                _UnityWebView.Close();
            }
        }
    }

}
