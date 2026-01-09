/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using FH.WV;

#if UNITY_EDITOR && UNITY_EDITOR_WIN
using PlatformWebViewMgr = FH.WV.PlatformWebViewMgr_Windows;
#elif UNITY_EDITOR && UNITY_EDITOR_OSX
    using PlatformWebViewMgr = FH.WV.PlatformWebViewMgr_Empty;    
#elif UNITY_EDITOR 
    using PlatformWebViewMgr = FH.WV.PlatformWebViewMgr_Empty;
#elif UNITY_STANDALONE_WIN
    using PlatformWebViewMgr = FH.WV.PlatformWebViewMgr_Windows;
#elif UNITY_ANDROID
    using PlatformWebViewMgr = FH.WV.PlatformWebViewMgr_Android;
#elif UNITY_IOS
    using PlatformWebViewMgr = FH.WV.PlatformWebViewMgr_IOS;
#else
    using PlatformWebViewMgr = FH.WV.PlatformWebViewMgr_Empty;
#endif

namespace FH
{  
    public static class WebViewMgr
    {
        private static Dictionary<int, WebView> _Dict = new();
        private static FH.WV.IPlatformWebViewMgr _;
        private static FH.WV.IPlatformWebViewMgr Inst
        {
            get
            {
                if (_ != null)
                    return _;
                _ = new PlatformWebViewMgr();
                _.SetWebViewCallBack(UnityWebViewHandler.Init());
                return _;
            }
        }         
 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="rect">normalized rect, [0,1], LeftTop is (0,0), RightBottom is (1,1)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static WebView Create(string url, Rect normalizedRect)
        {
            int id = Inst.Create(url, normalizedRect);

            if (id == 0)
                return new WebView(Inst, 0);

            var ret = new WebView(Inst, id);
            _Dict[id] = ret;
            return ret;
        }

        internal static void OnJsMsg(int webViewId, string message)
        {
            WebViewLog._.I($"{message}");
        }

        internal static void OnWebViewEvent(int webViewId, EWebViewEventType eventType)
        {
            WebViewLog._.D($"OnWebViewEvent {webViewId} {eventType}");

            _Dict.TryGetValue(webViewId, out WebView webView);
            if (webView == null)
                return;

            if (eventType == EWebViewEventType.Destroyed)
                _Dict.Remove(webViewId);

            webView.OnEvent(eventType);
        }
    }
}

