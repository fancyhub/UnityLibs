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
    public class WebView_Empty : IWebView
    {
        public bool CanClearCache() { return false; }

        public bool CanClearCookies() { return false; }

        public void ClearCache() { }

        public void ClearCookies() { }

        public void Close(int webViewId) { }

        public void CloseAll() { }

        public void DeleteLocalStorage() { }

        public string GetURL(int webViewId) { return null; }

        public void GoBack(int webViewId) { }

        public void GoForward(int webViewId) { }

        public bool IsLoading(int webViewId) { return false; }

        public bool IsVisible(int webViewId) { return false; }

        public int Open(string url, Rect normalizedRect, WebViewParameters parameters) { return 0; }

        public void Reload(int webViewId) { }

        public void Resize(int webViewId, Rect normalizedRect) { }

        public void RunJavaScript(int webViewId, string jsCode) { }

        public void SetEnv(WebViewEnv config) { }

        public void SetVisible(int webViewId, bool visible) { }
    }
}

