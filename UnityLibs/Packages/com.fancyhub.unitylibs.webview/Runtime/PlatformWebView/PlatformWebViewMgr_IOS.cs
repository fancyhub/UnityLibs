/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

#if UNITY_IOS || UNITY_EDITOR
namespace FH.WV
{
    
    public class PlatformWebViewMgr_IOS : IPlatformWebViewMgr
    {
        private static partial class _
        {
            [DllImport("__Internal")]
            public static extern string getWebViewDeviceSystemVersion();
            [DllImport("__Internal")]
            public static extern void setWebViewUnityGameObjectName(string name);
            [DllImport("__Internal")]
            public static extern void createUserScript(string key, string script, int injectionTime, bool mainFrameOnly);
            [DllImport("__Internal")]
            public static extern void deleteUserScript(string key);
            [DllImport("__Internal")]
            public static extern int openWebView(string url, float x, float y, float width, float height, string parameters);
            [DllImport("__Internal")]
            public static extern void closeWebView(int webViewId);
            [DllImport("__Internal")]
            public static extern void closeAllWebViews();
            [DllImport("__Internal")]
            public static extern void moveWebView(int webViewId, float x, float y, float width, float height);
            [DllImport("__Internal")]
            public static extern bool canWebViewGoBackward(int webViewId);
            [DllImport("__Internal")]
            public static extern bool canWebViewGoForward(int webViewId);
            [DllImport("__Internal")]
            public static extern void makeWebViewGoBackward(int webViewId);
            [DllImport("__Internal")]
            public static extern void makeWebViewGoForward(int webViewId);
            [DllImport("__Internal")]
            public static extern void reloadWebView(int webViewId);
            [DllImport("__Internal")]
            public static extern string getWebViewURL(int webViewId);
            [DllImport("__Internal")]
            public static extern double getWebViewLoadingProgress(int webViewId);
            [DllImport("__Internal")]
            public static extern bool isWebViewLoading(int webViewId);
            [DllImport("__Internal")]
            public static extern void setNameAndCallbackInJavaScript_WebView(string name, string callback);
            [DllImport("__Internal")]
            public static extern string runJavaScript_WebView(int webViewId, string code, string callback, string id);
            [DllImport("__Internal")]
            public static extern bool canClearWebViewCookies();
            [DllImport("__Internal")]
            public static extern void clearWebViewCookies();
            [DllImport("__Internal")]
            public static extern bool canClearWebViewCache();
            [DllImport("__Internal")]
            public static extern void clearWebViewCache();
            [DllImport("__Internal")]
            public static extern void showWebView(int webViewId);
            [DllImport("__Internal")]
            public static extern void hideWebView(int webViewId);
            [DllImport("__Internal")]
            public static extern bool canCaptureScreenshot();
            [DllImport("__Internal")]
            public static extern bool captureScreenshot(int webViewId, string fileName);

            [DllImport("__Internal")]
            public static extern bool supportSafari();
            [DllImport("__Internal")]
            public static extern void setSafariUnityGameObjectName(string gameObjectName);
            [DllImport("__Internal")]
            public static extern void setSafariBarTintColor(float red, float green, float blue, float alpha);
            [DllImport("__Internal")]
            public static extern void setSafariControlTintColor(float red, float green, float blue, float alpha);
            [DllImport("__Internal")]
            public static extern void openSafari(string url, bool animated);
            [DllImport("__Internal")]
            public static extern void closeSafari(bool animated);
        }

        private WebViewEnv _Config;
        private bool _Inited = false;
        public void SetEnv(WebViewEnv config)
        {
            if (_Inited)
                return;
            _Inited = true;
            _Config = config;
        }

        public string GetWebViewDeviceSystemVersion()
        {
            return _.getWebViewDeviceSystemVersion();
        }

        public void SetUnitySendMessageGameObjectName(string name)
        {
            _.setWebViewUnityGameObjectName(name);
            _.setSafariUnityGameObjectName(name);
        }

        private bool _SendConsoleMessagesToUnity = false;
        public void SetSendConsoleMessagesToUnity(bool send)
        {
            if (!string.IsNullOrEmpty(_Config.JavascriptHostObjName) && (_SendConsoleMessagesToUnity != send))
            {
                string key = UNITY_LOGGER;
                if (!send)
                {
                    _.deleteUserScript(key);
                }
                else
                {
                    _.setNameAndCallbackInJavaScript_WebView(UNITY_LOGGER, "OnConsoleMessage_iOS");
                    _.createUserScript(key,
@"
function logToUnity(type, args)
{
    window.webkit.messageHandlers." + UNITY_LOGGER + @".postMessage(JSON.stringify({
        type: type,
        message: Array.from(args)
    }));
}

let __consoleDebug = console.debug
let __consoleLog = console.log
let __consoleWarn = console.warn
let __consoleError = console.error

console.debug = function() { logToUnity('debug', arguments); __consoleDebug.apply(null, arguments) }
console.log   = function() { logToUnity('log',   arguments); __consoleLog.apply(null, arguments) }
console.warn  = function() { logToUnity('warn',  arguments); __consoleWarn.apply(null, arguments) }
console.error = function() { logToUnity('error', arguments); __consoleError.apply(null, arguments) }

window.addEventListener('error', function(e) {
    logToUnity('uncaught', [`${e.message} at ${e.filename}:${e.lineno}:${e.colno}`])
})
",
                        USER_SCRIPT_INJECTION_TIME_DOC_START, false);

                }
                _SendConsoleMessagesToUnity = send;
            }
        }

        private const int USER_SCRIPT_INJECTION_TIME_DOC_START = 0;
        private const int USER_SCRIPT_INJECTION_TIME_DOC_END = 1;


        private const string UNITY_LOGGER = "unityLogger";
        private const string USER_SCRIPT_KEY_DISABLE_SCALING = "DisableScaling";
        public int Open(string url, float x, float y, float width, float height, WebViewParameters parameters)
        {
            string parameterString = string.Empty;
            if (parameters != null) parameterString = JsonUtility.ToJson(parameters);

            if (parameters != null)
            {
                if (parameters.Scaling)
                {
                    _.deleteUserScript(USER_SCRIPT_KEY_DISABLE_SCALING);
                }
                else
                {
                    _.createUserScript(USER_SCRIPT_KEY_DISABLE_SCALING, @"
                        var meta = document.createElement('meta');
                        meta.setAttribute('name', 'viewport');
                        meta.setAttribute('content', 'width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no');
                        document.getElementsByTagName('head')[0].appendChild(meta);"
                        , USER_SCRIPT_INJECTION_TIME_DOC_END, true);
                }
            }



            return _.openWebView(url, x, 1 - y - height, width, height, parameterString);
        }

        public void Close(int webViewId)
        {
            _.closeWebView(webViewId);
        }

        public void CloseAll()
        {
            _.closeAllWebViews();
        }

        public void Move(int webViewId, float x, float y, float width, float height)
        {
            _.moveWebView(webViewId, x, 1 - y - height, width, height);
        }

        public bool CanGoBackward(int webViewId)
        {
            return _.canWebViewGoBackward(webViewId);
        }

        public bool CanGoForward(int webViewId)
        {
            return _.canWebViewGoForward(webViewId);
        }

        public void GoBackward(int webViewId)
        {
            _.makeWebViewGoBackward(webViewId);
        }
        public void GoForward(int webViewId)
        {
            _.makeWebViewGoForward(webViewId);
        }

        public void Reload(int webViewId)
        {
            _.reloadWebView(webViewId);
        }

        public string GetURL(int webViewId)
        {
            return _.getWebViewURL(webViewId);
        }

        public float GetLoadingProgress(int webViewId)
        {
            return (float)_.getWebViewLoadingProgress(webViewId);
        }

        public bool IsLoading(int webViewId)
        {
            return _.isWebViewLoading(webViewId);
        }

        public void SetNameInJavaScript(string name)
        {
            _.setNameAndCallbackInJavaScript_WebView(name, "OnJavaScriptPostMessage");
        }

        public string RunJavaScript(int webViewId, string jsCode, string callback, string id)
        {
            return _.runJavaScript_WebView(webViewId, jsCode, callback, id);
        }

        public bool CanClearCookies()
        {
            return _.canClearWebViewCookies();
        }
        public void ClearCookies()
        {
            _.clearWebViewCookies();
        }

        public bool CanClearCache()
        {
            return _.canClearWebViewCache();
        }

        public void ClearCache()
        {
            _.clearWebViewCache();
        }

        public void Show(int webViewId)
        {
            _.showWebView(webViewId);

            if (!string.IsNullOrEmpty(_Config.JavascriptHostObjName) && _Config.UseMediaManipulationOnHideAndShowByJavaScript)
            {
                _.runJavaScript_WebView(webViewId,
@"
var videos = document.getElementsByTagName('video');
for( var i = 0; i < videos.length; i++ )
{
    if(videos.item(i).getAttribute('_paused_during_hidden_') == 1)
    {
        videos.item(i).removeAttribute('_paused_during_hidden_');
        videos.item(i).play();
    }
}
var audios = document.getElementsByTagName('audio');
for( var i = 0; i < audios.length; i++ )
{
    if(audios.item(i).getAttribute('_paused_during_hidden_') == 1)
    {
        audios.item(i).removeAttribute('_paused_during_hidden_');
        audios.item(i).play();
    }
}",
                     string.Empty, string.Empty);
            }
        }
        public void Hide(int webViewId)
        {
            if (!string.IsNullOrEmpty(_Config.JavascriptHostObjName) && _Config.UseMediaManipulationOnHideAndShowByJavaScript)
            {
                _.runJavaScript_WebView(webViewId,
@"
var videos = document.getElementsByTagName('video');
for( var i = 0; i < videos.length; i++ )
{
    if(!videos.item(i).paused)
    {
        videos.item(i).setAttribute('_paused_during_hidden_', 1); 
        videos.item(i).pause();
    }
}
var audios = document.getElementsByTagName('audio');
for( var i = 0; i < audios.length; i++ )
{
    if(!audios.item(i).paused)
    {
        audios.item(i).setAttribute('_paused_during_hidden_', 1); 
        audios.item(i).pause();
    }
}",
                     string.Empty, string.Empty);
            }

            _.hideWebView(webViewId);
        }

        public bool CanCaptureScreenshot()
        {
            return _.canCaptureScreenshot();
        }

        public bool CaptureScreenshot(int webViewId, string fileName)
        {
            return _.captureScreenshot(webViewId, fileName);
        }

        public bool SupportSafari()
        {
            return _.supportSafari();
        }

        public void OpenSafari(string url, bool animated)
        {
            _.openSafari(url, animated);
        }

        public void CloseSafari(bool animated)
        {
            _.closeSafari(animated);
        }

        public void DeleteLocalStorage()
        {
            throw new NotImplementedException();
        }

        public void SetUserAgentString(int webViewId, string userAgentString)
        {
            throw new NotImplementedException();
        }

        public int Open(string url, Rect normalizedRect, WebViewParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void Resize(int webViewId, Rect normalizedRect)
        {
            throw new NotImplementedException();
        }

        public void RunJavaScript(int webViewId, string jsCode)
        {
            throw new NotImplementedException();
        }

        public void GoBack(int webViewId)
        {
            throw new NotImplementedException();
        }

        public void SetVisible(int webViewId, bool visible)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(int webViewId)
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

        public int Create(string url, Rect normalizedRect)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.SetEventCallBack(IPlatformWebViewMgr.OnWebViewEvent eventCallBack)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.SetEnv(WebViewEnv config)
        {
            throw new NotImplementedException();
        }

        int IPlatformWebViewMgr.Create(string url, Rect normalizedRect)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.Navigate(int webviewId, string url)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.SetBGColor(int webviewId, Color32 color)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.Resize(int webViewId, Rect normalizedRect)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.Close(int webViewId)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.RunJavaScript(int webViewId, string jsCode)
        {
            throw new NotImplementedException();
        }

        string IPlatformWebViewMgr.GetURL(int webViewId)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.Reload(int webViewId)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.GoBack(int webViewId)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.GoForward(int webViewId)
        {
            throw new NotImplementedException();
        }

        bool IPlatformWebViewMgr.IsLoading(int webViewId)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.SetVisible(int webViewId, bool visible)
        {
            throw new NotImplementedException();
        }

        bool IPlatformWebViewMgr.IsVisible(int webViewId)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.CloseAll()
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
    //*/
}

#endif