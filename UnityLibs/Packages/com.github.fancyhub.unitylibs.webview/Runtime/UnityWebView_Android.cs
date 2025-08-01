using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

#if UNITY_ANDROID || UNITY_EDITOR  
namespace FH.WV
{
    internal class WebView_Android : IWebView
    {
        private static bool _Inited = false;
        private static AndroidJavaClass _WebViewManager;
        private static AndroidJavaClass WebViewManager
        {
            get
            {
                if (_WebViewManager == null)
                {
                    _WebViewManager = new AndroidJavaClass("com.github.fancyhub.webview.WebViewManager");
                }
                return _WebViewManager;
            }
        }

        public void Fix()
        {
            Application.OpenURL("http://play.google.com/store/apps/details?id=com.google.android.webview");
        }

        public void Init(WebViewConfig config)
        {
            if (_Inited)
                return;

            _Inited = true;
            WebViewManager.CallStatic("Init", config.UnityHandlerName);            
            WebViewManager.CallStatic("extraLog", true);
        }


        public int Open(string url, Rect pos, WebViewParameters parameters)
        {
            string parameterString = string.Empty;
            if (parameters != null) 
                parameterString = JsonUtility.ToJson(parameters);
            var ret = WebViewManager.CallStatic<int>("open", url, pos.x, pos.y, pos.width, pos.height, parameterString);

            return ret;
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

        public void GoBackward(int webViewId)
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

        public void Show(int webViewId)
        {
            WebViewManager.CallStatic("show", webViewId);
        }

        public void Hide(int webViewId)
        {
            WebViewManager.CallStatic("hide", webViewId);
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
    }
}

#endif