/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using FH.UI;

namespace FH
{
    public interface IWebViewListener
    {
        public void OnWebViewLoaded(WebView webView);
        public void OnWebViewClosed(WebView webView);
    }

    public class WebView
    {
        private FH.WV.IPlatformWebViewMgr _PlatformWebViewMgr;
        private int _WebViewId;
        private IWebViewListener _Listener;

        internal WebView(FH.WV.IPlatformWebViewMgr mgr, int webviewId)
        {
            _PlatformWebViewMgr = mgr;
            _WebViewId = webviewId;
        }

        public void SetListener(IWebViewListener listener)
        {
            _Listener = listener;
        }

        public void Resize(Rect normalizedRect)
        {
            _PlatformWebViewMgr?.Resize(_WebViewId, normalizedRect);
        }

        public void Navigate(string url)
        {
            _PlatformWebViewMgr?.Navigate(_WebViewId, url);
        }

        public bool IsValid()
        {
            return _PlatformWebViewMgr != null && _WebViewId != 0;
        }

        public void Close()
        {
            if (_WebViewId == 0)
                return;
            _PlatformWebViewMgr?.Close(_WebViewId);
            _PlatformWebViewMgr = null;
        }

        public void Reload()
        {
            _PlatformWebViewMgr?.Reload(_WebViewId);
        }

        public void GoBackward()
        {
            _PlatformWebViewMgr?.GoBack(_WebViewId);
        }

        public void GoForward()
        {
            _PlatformWebViewMgr?.GoForward(_WebViewId);
        }

        public string GetURL()
        {
            if (_PlatformWebViewMgr == null)
                return string.Empty;
            return _PlatformWebViewMgr.GetURL(_WebViewId);
        }

        public bool IsLoading()
        {
            if (_PlatformWebViewMgr == null)
                return false;
            return _PlatformWebViewMgr.IsLoading(_WebViewId);
        }

        public void SetVisible(bool visible)
        {
            _PlatformWebViewMgr?.SetVisible(_WebViewId, visible);
        }

        public bool IsVisible()
        {
            if (_PlatformWebViewMgr == null)
                return false;
            return _PlatformWebViewMgr.IsVisible(_WebViewId);
        }

        public void RunJavaScript(string jsCode)
        {
            _PlatformWebViewMgr?.RunJavaScript(_WebViewId, jsCode);
        }

        internal void OnEvent(EWebViewEventType eventType)
        {
            WebViewLog._.D("WebView {0} Event {1}", _WebViewId, eventType);

            switch (eventType)
            {
                case EWebViewEventType.Destroyed:
                    _PlatformWebViewMgr = null;
                    _Listener?.OnWebViewClosed(this);
                    _WebViewId = 0;
                    break;

                case EWebViewEventType.DocumentReady:
                    _Listener?.OnWebViewLoaded(this);
                    break;
            }
        }
    }
}
