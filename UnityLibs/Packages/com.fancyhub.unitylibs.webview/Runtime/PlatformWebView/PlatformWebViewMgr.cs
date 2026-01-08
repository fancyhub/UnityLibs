/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;

namespace FH.WV
{
    using WebViewId = System.Int32;    

    internal interface IPlatformWebViewMgrCallback
    {
        public void OnWebViewEvent(WebViewId webViewId, EWebViewEventType eventType);
        public void OnJsMsg(WebViewId webViewId, string message);
    }

    internal interface IPlatformWebViewMgr
    {
        public void SetWebViewCallBack(IPlatformWebViewMgrCallback webViewCallback);

        public void SetEnv(WebViewEnv config);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="normalizedRect">normalized rect, [0,1], LeftTop is (0,0), RightBottom is (1,1)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public WebViewId Create(string url, Rect normalizedRect);

        public void Navigate(WebViewId webViewId, string url);

        public void SetBGColor(WebViewId webViewId, Color32 color);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="normalizedRect">normalized rect, [0,1], LeftTop is (0,0), RightBottom is (1,1)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public void Resize(WebViewId webViewId, Rect normalizedRect);

        public void Close(WebViewId webViewId);
        public void RunJavaScript(WebViewId webViewId, string jsCode);

        public string GetURL(WebViewId webViewId);
        public void Reload(WebViewId webViewId);

        public void GoBack(WebViewId webViewId);
        public void GoForward(WebViewId webViewId);
        public bool IsLoading(WebViewId webViewId);

        public void SetVisible(WebViewId webViewId, bool visible);
        public bool IsVisible(WebViewId webViewId);

        public void CloseAll();        

    }
}

