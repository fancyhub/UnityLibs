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
    internal class UnityWebViewHandler : MonoBehaviour, IPlatformWebViewMgrCallback
    {
        public enum EEventType
        {
            Message,
            Event,
            InnerLog,
        }

        public struct MessageData
        {
            public EEventType type;
            public int webViewId;
            public string message;
            public EWebViewEventType webviewEventType;
            public ELogLvl logLvl;
        }

        private static UnityWebViewHandler _;


        private static List<MessageData> _MessageQueue = new List<MessageData>();

        private Queue<MessageData> _TempList = new Queue<MessageData>();

        public static UnityWebViewHandler Init()
        {
            if (_ != null)
                return _;
            GameObject handler = new GameObject();
            _ = handler.AddComponent<UnityWebViewHandler>();
            handler.name = "_UnityWebViewHandler";
            handler.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(handler);
            return _;
        }

        void Update()
        {
            lock (_MessageQueue)
            {
                foreach (var p in _MessageQueue)
                {
                    _TempList.Enqueue(p);
                }
                _MessageQueue.Clear();
            }

            for (; ; )
            {
                if (_TempList.Count == 0)
                    break;

                var msg = _TempList.Dequeue();

                switch (msg.type)
                {
                    case EEventType.Message:
                        WebViewMgr.OnJsMsg(msg.webViewId, msg.message);
                        break;

                    case EEventType.Event:
                        WebViewMgr.OnWebViewEvent(msg.webViewId, msg.webviewEventType);
                        break;

                    case EEventType.InnerLog:
                        switch (msg.logLvl)
                        {
                            case ELogLvl.Debug:
                                WebViewLog._.D(msg.message);
                                break;

                            case ELogLvl.Info:
                                WebViewLog._.I(msg.message);
                                break;

                            case ELogLvl.Warning:
                                WebViewLog._.W(msg.message);
                                break;


                            default:
                            case ELogLvl.Error:
                                WebViewLog._.E(msg.message);
                                break;
                        }
                        break;
                }
            }
        }


        void IPlatformWebViewMgrCallback.OnJsMsg(int webViewId, string message)
        {
            if (this == null)
                return;

            if (string.IsNullOrEmpty(message))
                return;

            lock (_MessageQueue)
            {
                _MessageQueue.Add(new MessageData()
                {
                    type = EEventType.Message,
                    webViewId = webViewId,
                    message = message
                });
            }
        }

        void IPlatformWebViewMgrCallback.OnWebViewEvent(int webViewId, EWebViewEventType eventType)
        {
            lock (_MessageQueue)
            {
                _MessageQueue.Add(new MessageData()
                {
                    type = EEventType.Event,
                    webViewId = webViewId,
                    webviewEventType = eventType
                });
            }
        }

        void IPlatformWebViewMgrCallback.OnInnerlLog(ELogLvl lvl, string msg)
        {
            lock (_MessageQueue)
            {
                _MessageQueue.Add(new MessageData()
                {
                    type = EEventType.InnerLog,
                    logLvl = lvl,
                    message = msg,
                });
            }
        }
    }
}

