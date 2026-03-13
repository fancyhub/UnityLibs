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


namespace FH
{
    public static partial class WebViewMgr
    {
        private static void _RegJsHandler()
        {
            WebView.RegGlobalJsMessageHandler("Echo", _JsHandler_Echo);
            WebView.RegGlobalJsMessageHandler("GetVersion", _JsHandler_GetVersion);
            WebView.RegGlobalJsMessageHandler("GetNetworkType", _JsHandler_GetNetworkType);
        }

        private static void _JsHandler_Echo(WebView view, string key, string promissId, string data)
        {
            Debug.Log("Echo Proccess " + promissId);
            view.ReturnToJs(promissId, data);
        }

        private static void _JsHandler_GetVersion(WebView view, string key, string promissId, string data)
        {
            view.ReturnToJs(promissId, Application.version);
        }

        private static void _JsHandler_GetNetworkType(WebView view, string key, string promissId, string data)
        {
            view.ReturnToJs(promissId, Application.internetReachability.ToString());
        }
    }
}

