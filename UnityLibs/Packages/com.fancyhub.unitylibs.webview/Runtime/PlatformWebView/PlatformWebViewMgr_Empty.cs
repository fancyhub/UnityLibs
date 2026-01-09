/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.WV
{
    public class PlatformWebViewMgr_Empty : IPlatformWebViewMgr
    {
        void IPlatformWebViewMgr.Close(int webViewId) { }

        void IPlatformWebViewMgr.CloseAll() { }

        int IPlatformWebViewMgr.Create(string url, Rect normalizedRect) { return 0; }

        string IPlatformWebViewMgr.GetURL(int webViewId) { return null; }

        void IPlatformWebViewMgr.GoBack(int webViewId) { }

        void IPlatformWebViewMgr.GoForward(int webViewId) { }

        bool IPlatformWebViewMgr.IsLoading(int webViewId) { return false; }

        bool IPlatformWebViewMgr.IsVisible(int webViewId) { return false; }

        void IPlatformWebViewMgr.Navigate(int webviewId, string url) { }

        void IPlatformWebViewMgr.Reload(int webViewId) { }

        void IPlatformWebViewMgr.Resize(int webViewId, Rect normalizedRect) { }

        void IPlatformWebViewMgr.RunJavaScript(int webViewId, string jsCode) { }

        void IPlatformWebViewMgr.SetBGColor(int webviewId, Color32 color) { }

        void IPlatformWebViewMgr.SetGlobalBGColor(Color32 color) { }

        void IPlatformWebViewMgr.SetGlobalScaling(bool scaling) { }

        void IPlatformWebViewMgr.SetGlobalUserAgent(string userAgent) { }

        void IPlatformWebViewMgr.SetVisible(int webViewId, bool visible) { }
         

        void IPlatformWebViewMgr.SetWebViewCallBack(IPlatformWebViewMgrCallback webViewCallback) { }
    }
}

