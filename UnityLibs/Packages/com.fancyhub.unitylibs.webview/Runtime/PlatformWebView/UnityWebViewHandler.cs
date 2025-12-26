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
    internal class UnityWebViewHandler : MonoBehaviour
    {
        public enum EEventType
        {
            Message,
            Event,
        }

        public struct MessageData
        {
            public EEventType type;
            public int webviewId;
            public string message;
            public int webviewEventType;
        }

        private static UnityWebViewHandler _;

        private static IPlatformWebViewMgr _platformWebViewMgr;
        private static List<MessageData> _MessageQueue = new List<MessageData>();

        private Queue<MessageData> _TempList = new Queue<MessageData>();

        public static UnityWebViewHandler Init(IPlatformWebViewMgr mgr)
        {
            if (_ != null)
                return _;
            _platformWebViewMgr = mgr;
            GameObject handler = new GameObject();
            _ = handler.AddComponent<UnityWebViewHandler>();
            handler.name = WebViewDef.HandlerName;
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

            if (_platformWebViewMgr == null)
            {
                _TempList.Clear();
                return;
            }

            for (; ; )
            {
                if (_TempList.Count == 0)
                    break;

                var msg = _TempList.Dequeue();

                switch (msg.type)
                {
                    case EEventType.Message:
                        _platformWebViewMgr.OnMessage(msg.webviewId, msg.message);
                        break;

                    case EEventType.Event:
                        _platformWebViewMgr.OnEvent(msg.webviewId, msg.webviewEventType);
                        break;
                }
            }
        }

        public void OnWebViewEvent(int webviewId, int eventType)
        {
            lock (_MessageQueue)
            {
                _MessageQueue.Add(new MessageData()
                {
                    type = EEventType.Event,
                    webviewId = webviewId,
                    webviewEventType = eventType
                });
            }
        }

        public void OnWebViewMsg(int webviewId, string content)
        {
            if (string.IsNullOrEmpty(content))
                return;

            lock (_MessageQueue)
            {
                _MessageQueue.Add(new MessageData()
                {
                    type = EEventType.Message,
                    webviewId = webviewId,
                    message = content
                });
            }
        }

        public void OnWebViewLog(string log)
        {
            WebViewLog._.D(log);
        }

        public void OnWebViewInfo(string log)
        {
            WebViewLog._.I(log);
        }

        public void OnWebViewError(string log)
        {
            WebViewLog._.E(log);
        }
    }
}

