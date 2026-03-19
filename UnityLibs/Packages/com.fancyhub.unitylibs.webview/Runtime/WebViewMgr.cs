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
    public static partial class WebViewMgr
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
                _RegJsHandler();
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


        #region JS Promise
        public delegate void JsMessageHandler(WebView view, string key, string promissId, string data);
        private struct JsMessageHandlerData
        {
            public JsMessageHandler _Handler;
            public JsMessageHandlerData(JsMessageHandler handler) { _Handler = handler; }
        }
        private static Dictionary<string, JsMessageHandlerData> _GlobalJsMessageHandler = new();

        internal class InternalMsgData
        {
            public string key;
            public string promiseId;
            public string data;

            //public static InternalMsgData ParseWithNetJson(string msg)
            //{
            //    var result =  System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(msg);

            //    InternalMsgData result = new ();
            //    result.key = result.GetProperty("key").GetString();
            //    result.promissId = result.GetProperty("promissId").GetString();
            //    result.data = result.GetProperty("data").GetRawText();
            //    return result;
            //}

            //public static InternalMsgData ParseWithNewton(string msg)
            //{
            //    // 解析成 JObject
            //    Newtonsoft.Json.JObject jObj = Newtonsoft.Json.JObject.Parse(msg);

            //    InternalMsgData result = new ();
            //    result.key = jObj["key"].ToString();
            //    result.promissId = jObj["promissId"].ToString();                
            //    result.data = jObj["data"].ToString();

            //    return result;
            //}


            public static InternalMsgData ParseWithSimpleJson(string msg)
            {
                SimpleJSON.JSONNode rootNode = SimpleJSON.JSON.Parse(msg);
                if (rootNode == null)
                {
                    WebViewLog._.E("@ParseWithSimpleJson JSON.parse() returned null!");
                    return null;
                }

                SimpleJSON.JSONNode nodeKey = rootNode[nameof(key)];
                if (nodeKey == null)
                {
                    WebViewLog._.E("@ParseWithSimpleJson JSON.parse() returned null! no key");
                    return null;
                }

                SimpleJSON.JSONNode nodePromissId = rootNode[nameof(promiseId)];
                SimpleJSON.JSONNode nodeData = rootNode[nameof(data)];

                InternalMsgData ret = new InternalMsgData();
                ret.key = nodeKey;

                if (nodePromissId != null)
                    ret.promiseId = nodePromissId;

                if (nodeData != null)
                    ret.data = nodeData;

                return ret;
            }

        }

        internal static void OnJsMsg(int webViewId, string message)
        {
            WebViewLog.JsLog.D(message);

            //1. get webview
            _Dict.TryGetValue(webViewId, out WebView webView);
            if (webView == null)
            {
                WebViewLog._.Assert(false, "can't find web view {0}", webViewId);
                return;
            }

            //2. check message
            InternalMsgData msgData = null;
            {
                if (string.IsNullOrEmpty(message))
                {
                    WebViewLog._.Assert(false, "js message is null {0}}", webViewId);
                    return;
                }

                msgData = InternalMsgData.ParseWithSimpleJson(message);
                if (msgData == null)
                {
                    WebViewLog._.Assert(false, "js message struct is not correct {0}}", webViewId);
                    return;
                }
            }

            //3. process
            bool processed = false;
            if (_GlobalJsMessageHandler.TryGetValue(msgData.key, out var handler))
            {
                handler._Handler(webView, msgData.key, msgData.promiseId, msgData.data);
                processed = true;
            }

            if (webView.OnJsMsg(msgData.key, msgData.promiseId, msgData.data))
            {
                processed = true;
            }

            //4. check message is processed
            if (!processed)
            {
                WebViewLog._.W("{0} is not processed", msgData.key);
            }
        }

        public static void RegGlobalJsMessageHandler(string key, JsMessageHandler handler)
        {
            if (handler == null)
                return;
            _GlobalJsMessageHandler[key] = new JsMessageHandlerData(handler);
        }
        #endregion

        internal static void OnWebViewEvent(int webViewId, EWebViewEventType eventType)
        {
            WebViewLog._.D("OnWebViewEvent {0} {1}", webViewId, eventType);

            _Dict.TryGetValue(webViewId, out WebView webView);
            if (webView == null)
            {
                WebViewLog._.Assert(false, "can't find web view {0}", webViewId);
                return;
            }

            if (eventType == EWebViewEventType.Destroyed)
                _Dict.Remove(webViewId);

            webView.OnEvent(eventType);
        }
    }
}

