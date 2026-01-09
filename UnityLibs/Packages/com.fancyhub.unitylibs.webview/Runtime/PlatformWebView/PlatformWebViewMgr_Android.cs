/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_ANDROID || UNITY_EDITOR  
namespace FH.WV
{
    internal class PlatformWebViewMgr_Android : IPlatformWebViewMgr
    {
        private static AndroidJavaClass _WebViewManager;
        private static AndroidJavaClass WebViewManager
        {
            get
            {
                if (_WebViewManager == null)
                    _WebViewManager = new AndroidJavaClass("com.fancyhub.webview.WebViewManager");
                return _WebViewManager;
            }
        }

        internal class AndroidWebViewCallBack : AndroidJavaProxy
        {
            private IPlatformWebViewMgrCallback _CallBack;

            public AndroidWebViewCallBack()
                : base("com.fancyhub.webview.IWebViewManagerCallBack")
            {
                WebViewLog._.D($"AndroidWebViewCallBack New");
            }

            public void SetCallBack(IPlatformWebViewMgrCallback callBack)
            {
                _CallBack = callBack;
            }

            public void OnEvent(int webViewId, int eventType)
            {
                WebViewLog._.D($"OnEvent {webViewId} {(EWebViewEventType)eventType}");
                _CallBack?.OnWebViewEvent(webViewId, (EWebViewEventType)eventType);
            }

            public void OnInternalLog(int logLevel, string msg)
            {
                switch (logLevel)
                {
                    default:
                        WebViewLog._.E(msg);
                        break;

                    case 0:
                        WebViewLog._.D(msg);
                        break;

                    case 1:
                        WebViewLog._.I(msg);
                        break;

                    case 2: //warning
                        WebViewLog._.W(msg);
                        break;

                    case 3: //error
                        WebViewLog._.E(msg);
                        break;

                }
            }

            public void OnJsLog(int webViewId, string logType, string source, int lineNumber, string msg)
            {
                switch (logType)
                {
                    case "TIP":
                    case "DEBUG":
                        WebViewLog.JsLog.D("WebviewId: {0}, {1} @ {2} -> {3}", webViewId, logType, source, msg);
                        break;

                    case "LOG":
                        WebViewLog.JsLog.I("WebviewId: {0}, {1} @ {2} -> {3}", webViewId, logType, source, msg);
                        break;

                    case "WARNING":
                        WebViewLog.JsLog.W("WebviewId: {0}, {1} @ {2} -> {3}", webViewId, logType, source, msg);
                        break;

                    case "ERROR":
                        WebViewLog.JsLog.E("WebviewId: {0}, {1} @ {2} -> {3}", webViewId, logType, source, msg);
                        break;

                    default:
                        WebViewLog.JsLog.E("WebviewId: {0}, {1} @ {2} -> {3}", webViewId, logType, source, msg);
                        break;
                }
            }

            public void OnJsMessage(int webViewId, string msg)
            {
                WebViewLog._.D($"OnJsMessage {webViewId} {msg}");
                _CallBack?.OnJsMsg(webViewId, msg);
            }
        }

        private AndroidWebViewCallBack _CallBack;


        public PlatformWebViewMgr_Android()
        {
            _CallBack = new AndroidWebViewCallBack();
            WebViewManager.CallStatic("Init", WebViewDef.JsHostObjName, _CallBack);
        }

        void IPlatformWebViewMgr.Close(int webViewId)
        {
            WebViewManager.CallStatic("Close", webViewId);
        }

        void IPlatformWebViewMgr.CloseAll()
        {
            WebViewManager.CallStatic("CloseAll");
        }

        int IPlatformWebViewMgr.Create(string url, Rect normalizedRect)
        {
            return WebViewManager.CallStatic<int>("Create", url, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
        }

        string IPlatformWebViewMgr.GetURL(int webViewId)
        {
            return WebViewManager.CallStatic<string>("GetURL", webViewId);
        }

        void IPlatformWebViewMgr.GoBack(int webViewId)
        {
            WebViewManager.CallStatic("GoBack", webViewId);
        }

        void IPlatformWebViewMgr.GoForward(int webViewId)
        {
            WebViewManager.CallStatic("GoForward", webViewId);
        }

        bool IPlatformWebViewMgr.IsLoading(int webViewId)
        {
            return WebViewManager.CallStatic<bool>("IsLoading", webViewId);
        }

        bool IPlatformWebViewMgr.IsVisible(int webViewId)
        {
            return WebViewManager.CallStatic<bool>("IsVisible", webViewId);
        }

        void IPlatformWebViewMgr.Navigate(int webViewId, string url)
        {
            WebViewManager.CallStatic("Navigate", webViewId, url);
        }

        void IPlatformWebViewMgr.SetVisible(int webViewId, bool visible)
        {
            WebViewManager.CallStatic("SetVisible", webViewId, visible);
        }

        void IPlatformWebViewMgr.Reload(int webViewId)
        {
            WebViewManager.CallStatic("Reload", webViewId);
        }

        void IPlatformWebViewMgr.Resize(int webViewId, Rect normalizedRect)
        {
            WebViewManager.CallStatic("Resize", webViewId, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
        }

        void IPlatformWebViewMgr.RunJavaScript(int webViewId, string jsCode)
        {
            WebViewManager.CallStatic("RunJavaScript", webViewId, jsCode);
        }

        void IPlatformWebViewMgr.SetBGColor(int webViewId, Color32 color)
        {
            WebViewManager.CallStatic("SetBGColor", webViewId, _ConvertColor(color));
        }

        private static int _ConvertColor(Color32 color)
        {
            uint a = color.a;
            uint r = color.r;
            uint g = color.g;
            uint b = color.b;

            uint final_c = (a & 0xff) << 24 | (r & 0xff) << 16 | (g & 0xff) << 8 | (b & 0xff);
            return (int)final_c;
        }

        void IPlatformWebViewMgr.SetWebViewCallBack(IPlatformWebViewMgrCallback webViewCallback)
        {
            _CallBack?.SetCallBack(webViewCallback);
        }

        void IPlatformWebViewMgr.SetGlobalBGColor(Color32 color)
        {
            WebViewManager.CallStatic("SetGlobalBGColor", _ConvertColor(color));
        }

        void IPlatformWebViewMgr.SetGlobalUserAgent(string userAgent)
        {
            WebViewManager.CallStatic("SetGlobalUserAgent", userAgent);
        }

        void IPlatformWebViewMgr.SetGlobalScaling(bool scaling)
        {
            WebViewManager.CallStatic("SetGlobalScaling", scaling);
        }
    }
}

#endif