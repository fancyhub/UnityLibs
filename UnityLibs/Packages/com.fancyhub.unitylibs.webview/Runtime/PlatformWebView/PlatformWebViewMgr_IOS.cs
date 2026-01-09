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
        private  _.WebViewInternalLogCallBack _webViewInternalLogCallBack ;
        private _.WebViewEventCallBack _webViewEventCallBack ;
        private _.WebViewJsMessageCallBack _webViewJsMessageCallBack ;
        private _.WebViewJsLogCallBack _webViewJsLogCallBack ;
        public PlatformWebViewMgr_IOS()
        {
            _webViewInternalLogCallBack = _OnInternalLog;
            _.SetWebViewInternalLogCallBack(_webViewInternalLogCallBack);

            _webViewEventCallBack = _OnEvent;
            _.SetWebViewEventCallBack(_webViewEventCallBack);

            _webViewJsMessageCallBack = _OnJsMessage;
            _.SetWebViewJsMessageCallBack(_webViewJsMessageCallBack);

            _webViewJsLogCallBack = _OnJsLog;
            _.SetWebViewJSLogCallBack(_webViewJsLogCallBack);
        }

        int IPlatformWebViewMgr.Create(string url, Rect normalizedRect)
        {
            return _.Create(url, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
        }

        void IPlatformWebViewMgr.Close(int webViewId)
        {
            _.Close(webViewId);
        }

        void IPlatformWebViewMgr.CloseAll()
        {
            _.CloseAll();
        }

        string IPlatformWebViewMgr.GetURL(int webViewId)
        {
            return _.GetUrl(webViewId);
        }

        void IPlatformWebViewMgr.GoBack(int webViewId)
        {
            _.GoBack(webViewId);
        }

        void IPlatformWebViewMgr.GoForward(int webViewId)
        {
            _.GoForward(webViewId);
        }

        bool IPlatformWebViewMgr.IsLoading(int webViewId)
        {
            return _.IsLoading(webViewId);
        }

        bool IPlatformWebViewMgr.IsVisible(int webViewId)
        {
            return _.IsVisible(webViewId);
        }

        void IPlatformWebViewMgr.Navigate(int webViewId, string url)
        {
            _.Navigate(webViewId, url);
        }

        void IPlatformWebViewMgr.Reload(int webViewId)
        {
            _.Reload(webViewId);
        }

        void IPlatformWebViewMgr.Resize(int webViewId, Rect normalizedRect)
        {
            _.Resize(webViewId, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
        }

        void IPlatformWebViewMgr.RunJavaScript(int webViewId, string jsCode)
        {
            _.RunJsCode(webViewId, jsCode);
        }

        void IPlatformWebViewMgr.SetBGColor(int webViewId, Color32 color)
        {
            throw new NotImplementedException();
        }

        void IPlatformWebViewMgr.SetGlobalBGColor(Color32 color)
        {
            Color c = color;
            _.SetGlobalBGColor(c.r, c.g, c.b, c.a);
        }

        void IPlatformWebViewMgr.SetGlobalScaling(bool scaling)
        {
            _.SetGlobalScaling(scaling);
        }

        void IPlatformWebViewMgr.SetGlobalUserAgent(string userAgent)
        {
            _.SetGlobalUserAgent(userAgent);
        }

        void IPlatformWebViewMgr.SetVisible(int webViewId, bool visible)
        {
            _.SetVisible(webViewId, visible);
        }

        private IPlatformWebViewMgrCallback _CallBack;
        void IPlatformWebViewMgr.SetWebViewCallBack(IPlatformWebViewMgrCallback webViewCallback)
        {
            _CallBack = webViewCallback;
        }

        [AOT.MonoPInvokeCallback(typeof(_.WebViewInternalLogCallBack))]
        private void _OnInternalLog(int logLvl, IntPtr msgPtr)
        {
            string msg = Marshal.PtrToStringAnsi(msgPtr);
        }

        [AOT.MonoPInvokeCallback(typeof(_.WebViewEventCallBack))]
        private void _OnEvent(int webViewId, int eventType)
        {
            _CallBack?.OnWebViewEvent(webViewId, (EWebViewEventType)eventType);
        }

        [AOT.MonoPInvokeCallback(typeof(_.WebViewJsLogCallBack))]
        private void _OnJsLog(int webViewId, int logLvl, IntPtr msgPtr)
        {
            string msg = Marshal.PtrToStringAnsi(msgPtr);
        }

        [AOT.MonoPInvokeCallback(typeof(_.WebViewJsMessageCallBack))]
        private void _OnJsMessage(int webViewId, IntPtr msgPtr)
        {
            string msg = Marshal.PtrToStringAnsi(msgPtr);
            _CallBack?.OnJsMsg(webViewId, msg);
        }

        private static partial class _
        {
            public delegate void WebViewInternalLogCallBack(int logLvl, IntPtr msg);
            [DllImport("__Internal")] public static extern void SetWebViewInternalLogCallBack(WebViewInternalLogCallBack callBack);

            public delegate void WebViewEventCallBack(int webViewId, int eventType);
            [DllImport("__Internal")] public static extern void SetWebViewEventCallBack(WebViewEventCallBack callback);

            public delegate void WebViewJsLogCallBack(int webViewId, int logLvl, IntPtr msg);
            [DllImport("__Internal")] public static extern void SetWebViewJSLogCallBack(WebViewJsLogCallBack callBack);

            public delegate void WebViewJsMessageCallBack(int webViewId, IntPtr msg);
            [DllImport("__Internal")] public static extern void SetWebViewJsMessageCallBack(WebViewJsMessageCallBack callBack);

            [DllImport("__Internal")] public static extern void SetGlobalBGColor(float r, float g, float b, float a);
            [DllImport("__Internal")] public static extern void SetGlobalUserAgent(string userAgent);
            [DllImport("__Internal")] public static extern void SetGlobalScaling(bool scaling);
            [DllImport("__Internal")] public static extern void Init(string jsHostName);

            [DllImport("__Internal")] public static extern int Create(string url, float x, float y, float width, float height);
            [DllImport("__Internal")] public static extern void Close(int webViewId);
            [DllImport("__Internal")] public static extern void CloseAll();

            [DllImport("__Internal")] public static extern string GetUrl(int webViewId);
            [DllImport("__Internal")] public static extern void GoBack(int webViewId);
            [DllImport("__Internal")] public static extern void GoForward(int webViewId);

            [DllImport("__Internal")] public static extern void Navigate(int webViewId, string url);
            [DllImport("__Internal")] public static extern bool IsLoading(int webViewId);
            [DllImport("__Internal")] public static extern void Reload(int webViewId);

            [DllImport("__Internal")] public static extern void Resize(int webViewId, float x, float y, float width, float height);
            [DllImport("__Internal")] public static extern void SetVisible(int webViewId, bool visible);
            [DllImport("__Internal")] public static extern bool IsVisible(int webViewId);
            [DllImport("__Internal")] public static extern void SetBGColor(int webViewId, float r, float g, float b, float a);

            [DllImport("__Internal")] public static extern void RunJsCode(int webViewId, string jsCode);            
        }
    }
    //*/
}

#endif