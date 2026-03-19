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
        public void OnWebViewJsMsg(string key, string promissId, string data);
    }

    public partial class WebView
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
            WebViewLog._.D("Close Webview {0}", _WebViewId);
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

        public string GetTitle()
        {
            if (_PlatformWebViewMgr == null)
                return string.Empty;
            return _PlatformWebViewMgr.GetTitle(_WebViewId);
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

        internal bool OnJsMsg(string key, string promiseId, string data)
        {
            if (_Listener == null)
                return true;

            _Listener.OnWebViewJsMsg(key, promiseId, data);
            return false;
        }


        public void ReturnToJs(string promissId, string data, bool succ = true)
        {
            if (string.IsNullOrEmpty(promissId))
            {
                return;
            }

            string code = string.Format("unityReturn2Js('{0}',{1},{2})", promissId, succ ? "true" : "false", ToLiteral(data));
            Debug.Log(code);
            _PlatformWebViewMgr?.RunJavaScript(_WebViewId, code);
        }


        private static System.Text.StringBuilder _TempForLiteral;
        private static string ToLiteral(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (_TempForLiteral == null)
                _TempForLiteral = new System.Text.StringBuilder();
            _TempForLiteral.Clear();
            System.Text.StringBuilder literal = _TempForLiteral;
            literal.Append("\"");
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        // if (c >= 0x20 && c <= 0x7e) {
                        if (Char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.Control)
                        {
                            literal.Append(c);
                            // As UTF16 escaped character
                        }
                        else
                        {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            literal.Append("\"");
            return literal.ToString();
        }
    }
}
