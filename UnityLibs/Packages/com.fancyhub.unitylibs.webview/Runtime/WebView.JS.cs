/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/02/03
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using FH.UI;
using UnityEngine.Windows.Speech;

namespace FH
{
    public partial class WebView
    {
        public delegate void JsMessageHandler(WebView view, string key, string promissId, string data);
        private struct JsMessageHandlerData
        {
            public JsMessageHandler _Handler;
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


        public static void RegGlobalJsMessageHandler(string key, JsMessageHandler handler)
        {
            _GlobalJsMessageHandler[key] = new JsMessageHandlerData()
            {
                _Handler = handler
            };
        }

        internal void OnJsMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return;
            }

            var msgData = InternalMsgData.ParseWithSimpleJson(msg);
            if (msgData == null)
            {
                return;
            }

            bool processed = false;
            if (_GlobalJsMessageHandler.TryGetValue(msgData.key, out var handler))
            {
                handler._Handler(this, msgData.key, msgData.promiseId, msgData.data);
                processed = true;
            }

            if (_Listener != null)
            {
                _Listener.OnWebViewJsMsg(msgData.key, msgData.promiseId, msgData.data);
                processed = true;
            }

            if (!processed)
            {
                WebViewLog._.W("{0} is not processed", msgData.key);
            }
        }


        public void ReturnToJs(string promissId, string data, bool succ = true)
        {
            if (string.IsNullOrEmpty(promissId))
            {
                return;
            }

            string code = string.Format("unityReturn2Js('{0}',{1},{2})", promissId, succ ? "true" : "false", ToLiteral(data));
            Debug.Log(code);
            _PlatformWebViewMgr?.RunJavaScript(_WebViewId, code);
        }


        private static System.Text.StringBuilder _TempForLiteral;
        private static string ToLiteral(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (_TempForLiteral == null)
                _TempForLiteral = new System.Text.StringBuilder();
            _TempForLiteral.Clear();
            System.Text.StringBuilder literal = _TempForLiteral;
            literal.Append("\"");
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        // if (c >= 0x20 && c <= 0x7e) {
                        if (Char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.Control)
                        {
                            literal.Append(c);
                            // As UTF16 escaped character
                        }
                        else
                        {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        break;
                }
            }
            literal.Append("\"");
            return literal.ToString();
        }
    }
}
