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

        public PlatformWebViewMgr_Android()
        {
            WebViewManager.CallStatic("Init");
        }

        public void Close(int webViewId)
        {
            WebViewManager.CallStatic("Close", webViewId);
        }

        public void CloseAll()
        {
            WebViewManager.CallStatic("CloseAll");
        }

        public int Create(string url, Rect normalizedRect)
        {
            return WebViewManager.CallStatic<int>("Create", url, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
        }

        public string GetURL(int webViewId)
        {
            return WebViewManager.CallStatic<string>("GetURL", webViewId);
        }

        public void GoBack(int webViewId)
        {
            WebViewManager.CallStatic("GoBack", webViewId);
        }

        public void GoForward(int webViewId)
        {
            WebViewManager.CallStatic("GoForward", webViewId);
        }

        public bool IsLoading(int webViewId)
        {
            return WebViewManager.CallStatic<bool>("IsLoading", webViewId);
        }

        public bool IsVisible(int webViewId)
        {
            return WebViewManager.CallStatic<bool>("IsVisible", webViewId);
        }

        public void Navigate(int webViewId, string url)
        {
            WebViewManager.CallStatic("Navigate", webViewId, url);
        }

        public void SetVisible(int webViewId, bool visible)
        {
            WebViewManager.CallStatic("SetVisible", webViewId, visible);
        }

        public void Reload(int webViewId)
        {
            WebViewManager.CallStatic("Reload", webViewId);
        }

        public void Resize(int webViewId, Rect normalizedRect)
        {
            WebViewManager.CallStatic("Resize", webViewId, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
        }

        public void RunJavaScript(int webViewId, string jsCode)
        {
            WebViewManager.CallStatic("RunJavaScript", webViewId, jsCode);
        }

        public void SetBGColor(int webViewId, Color32 color)
        {
            WebViewManager.CallStatic("SetBGColor", webViewId, (int)color.r, (int)color.g, (int)color.b, (int)color.a);
        }

        public void SetEnv(WebViewEnv config)
        {
            
        }


        private IPlatformWebViewMgr.OnWebViewEvent _EventCallBack;
        public void SetEventCallBack(IPlatformWebViewMgr.OnWebViewEvent eventCallBack)
        {
            _EventCallBack = eventCallBack;
        }

        public void OnEvent(int webViewId, int eventType)
        {
            switch (eventType)
            {
                case 1:
                    _EventCallBack?.Invoke(webViewId, EWebViewEventType.DocumentReady);
                    break;

                case 2:
                    _EventCallBack?.Invoke(webViewId, EWebViewEventType.Destroyed);
                    break;

                default:
                    WebViewLog._.E("unkown event type {0}", eventType);
                    break;
            }
        }
         
        public void OnMessage(int webViewId, string message)
        {
            WebViewLog._.I($"{message}");
        }

    }

    internal class PlatformWebViewMgr_Android2
    {
        private static bool _Inited = false;
        private static AndroidJavaClass _WebViewManager;
        private static AndroidJavaClass WebViewManager
        {
            get
            {
                if (_WebViewManager == null)
                {
                    _WebViewManager = new AndroidJavaClass("com.fancyhub.webview.WebViewManager");
                }
                return _WebViewManager;
            }
        }

        public void Fix()
        {
            Application.OpenURL("http://play.google.com/store/apps/details?id=com.google.android.webview");
        }

        public void SetEnv(WebViewEnv config)
        {
            if (_Inited)
                return;

            _Inited = true;
            //WebViewManager.CallStatic("Init", config.UnityHandlerName);
            WebViewManager.CallStatic("extraLog", true);
        }


        public int Create(string url, Rect normalizedRect)
        {
            string parameterString = string.Empty;
            //if (parameters != null)
            //    parameterString = JsonUtility.ToJson(parameters);
            //var ret = WebViewManager.CallStatic<int>("open", url, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height, parameterString);

            //return ret;
            return 0;
        }

        public void Resize(int webViewId, Rect normalizedRect)
        {

        }


        public static bool Test()
        {
            return WebViewManager.CallStatic<bool>("test");
        }

        public void Close(int webViewId)
        {
            WebViewManager.CallStatic("close", webViewId);
        }

        public void CloseAll()
        {
            WebViewManager.CallStatic("closeAll");
        }

        public void Reload(int webViewId)
        {
            WebViewManager.CallStatic("reload", webViewId);
        }


        private static Dictionary<int, Action<int, bool>> _CanGoBackwardCallback = new Dictionary<int, Action<int, bool>>();
        internal static void OnCanGoBackward(int webViewId, bool result)
        {
            Action<int, bool> callback;
            if (_CanGoBackwardCallback.TryGetValue(webViewId, out callback))
            {
                callback.Invoke(webViewId, result);
                _CanGoBackwardCallback.Remove(webViewId);
            }
        }

        private static Dictionary<int, Action<int, bool>> _CanGoForwardCallback = new Dictionary<int, Action<int, bool>>();
        internal static void OnCanGoForward(int webViewId, bool result)
        {
            Action<int, bool> callback;
            if (_CanGoForwardCallback.TryGetValue(webViewId, out callback))
            {
                callback.Invoke(webViewId, result);
                _CanGoForwardCallback.Remove(webViewId);
            }
        }

        public void CanGoBackward(int webViewId, Action<int, bool> callback)
        {
            _CanGoBackwardCallback[webViewId] = callback;
            WebViewManager.CallStatic("canGoBackward", webViewId);
        }

        public void CanGoForward(int webViewId, Action<int, bool> callback)
        {
            _CanGoForwardCallback[webViewId] = callback;
            WebViewManager.CallStatic("canGoForward", webViewId);
        }

        public void GoBack(int webViewId)
        {
            WebViewManager.CallStatic("goBackward", webViewId);
        }

        public void GoForward(int webViewId)
        {
            WebViewManager.CallStatic("goForward", webViewId);
        }

        public string GetURL(int webViewId)
        {
            return WebViewManager.CallStatic<string>("getURL", webViewId);
        }

        public float GetLoadingProgress(int webViewId)
        {
            return (float)WebViewManager.CallStatic<double>("getLoadingProgress", webViewId);
        }

        public bool IsLoading(int webViewId)
        {
            return WebViewManager.CallStatic<bool>("isLoading", webViewId);
        }

        public void SetNameInJavaScript(string name)
        {
            WebViewManager.CallStatic("setNameInJavaScript", name);
        }

        public void RunJavaScript(int webViewId, string jsCode)
        {
            RunJavaScript(webViewId, jsCode, null, null);
        }

        public string RunJavaScript(int webViewId, string jsCode, string callback, string id)
        {
            return WebViewManager.CallStatic<string>("runJavaScript", webViewId, jsCode, callback, id);
        }

        public bool CanClearCookies()
        {
            return WebViewManager.CallStatic<bool>("canClearCookies");
        }

        public void ClearCookies()
        {
            WebViewManager.CallStatic("clearCookies");
        }

        public bool CanClearCache()
        {
            return WebViewManager.CallStatic<bool>("canClearCache");
        }

        public void ClearCache()
        {
            WebViewManager.CallStatic("clearCache");
        }

        public void DeleteLocalStorage()
        {
            WebViewManager.CallStatic("webStorage_DeleteAllData");
        }

        public void SetVisible(int webViewId, bool visible)
        {
            if (visible)
                WebViewManager.CallStatic("show", webViewId);
            else
                WebViewManager.CallStatic("hide", webViewId);
        }

        public bool IsVisible(int webViewId)
        {
            return false;
        }

        public bool CanCaptureScreenshot()
        {
            return true;
        }

        public bool CaptureScreenshot(int webViewId, string fileName)
        {
            return WebViewManager.CallStatic<bool>("captureScreenshot", webViewId, fileName);
        }
        private static Dictionary<int, Action<int, string>> _GetUserAgentStringCallback = new Dictionary<int, Action<int, string>>();
        internal static void OnGetUserAgentString(int webViewId, string userAgentString)
        {
            Action<int, string> callback;
            if (_GetUserAgentStringCallback.TryGetValue(webViewId, out callback))
            {
                callback.Invoke(webViewId, userAgentString);
                _GetUserAgentStringCallback.Remove(webViewId);
            }
        }
        public void GetUserAgentString(int webViewId, Action<int, string> callback)
        {
            _GetUserAgentStringCallback[webViewId] = callback;
            WebViewManager.CallStatic("getUserAgentString", webViewId);
        }

        public void SetUserAgentString(int webViewId, string userAgentString)
        {
            WebViewManager.CallStatic("setUserAgentString", webViewId, userAgentString);
        }

        public int Create(Rect normalizedRect)
        {
            throw new NotImplementedException();
        }

        public void Navigate(int webviewId, string url)
        {
            throw new NotImplementedException();
        }

        public void SetBGColor(int webviewId, Color32 color)
        {
            throw new NotImplementedException();
        }

        public void SetEventCallBack(IPlatformWebViewMgr.OnWebViewEvent eventCallBack)
        {
            throw new NotImplementedException();
        }

        public void OnMessage(int webViewId, string message)
        {
            throw new NotImplementedException();
        }

        public void OnEvent(int webViewId, int eventType)
        {
            throw new NotImplementedException();
        }
    }
}

#endif