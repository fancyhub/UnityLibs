/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/2/4
 * Title   : 
 * Desc    : Use WebView2 in windows
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using AOT;
using Microsoft.MixedReality.WebView;
using UnityEngine;

namespace FH
{
   
    //https://learn.microsoft.com/en-us/microsoft-edge/webview2/get-started/hololens2#step-2---install-unity-for-hololens-2-development
    public class WebViewWindows : FH.IWebView
    {
        private static Dictionary<IntPtr, WebViewWindows> _WebViewDict = new Dictionary<IntPtr, WebViewWindows>();
        private static SynchronizationContext UnityGameThreadContext;

        private IntPtr _InstId;
        private int _Width;
        private int _Height;
        private int _ActiveResizeRequestCount = 0;
        private UnityEngine.Texture2D _Texture;

        public string Url { get; private set; }
        public bool CanGoBack { get; private set; } = false;
        public bool CanGoForward { get; private set; } = false;

        public FH.IWebView.OnNavigated Navigated { get; set; }
        public FH.IWebView.OnPostMessage MessageReceived { get; set; }
        public FH.IWebView.OnCanGoForwardUpdated CanGoForwardUpdated { get; set; }
        public FH.IWebView.OnCanGoBackUpdated CanGoBackUpdated { get; set; }
        public FH.IWebView.OnNewWindowRequested NewWindowRequested { get; set; }
        public FH.IWebView.OnCloseRequested WindowCloseRequested { get; set; }
        public FH.IWebView.OnTextureReady TextureReady { get; set; }
        public FH.IWebView.OnReady Ready { get; set; }

        private WebViewWindows(IntPtr inst_id)
        {
            _InstId = inst_id;
        }

        public Vector2Int Size => new Vector2Int(_Width, _Height);

        public static WebViewWindows Create(int width, int height)
        {
            if (UnityGameThreadContext == null)
                UnityGameThreadContext = SynchronizationContext.Current;

            var InstanceId = WebViewNative.InitializeWebView(width, height);
            if (InstanceId == IntPtr.Zero)
                return null;

            WebViewNative.SetUrlChangedCallback(InstanceId, _OnUrlChangedCallback);
            WebViewNative.SetCanGoBackUpdatedCallback(InstanceId, _OnGoBackStatusUpdated);
            WebViewNative.SetCanGoForwardUpdatedCallback(InstanceId, _OnGoForwardUpdated);
            WebViewNative.SetPostMessageCallback(InstanceId, _OnPostMessageCallback);
            WebViewNative.SetReadyCallback(InstanceId, _OnReadyCallback);
            WebViewNative.SetTextureAvailableCallback(InstanceId, _OnTextureAvailable);
            WebViewNative.SetNewWindowRequestedCallback(InstanceId, _OnNewWindowRequested);
            WebViewNative.SetWindowCloseRequestedCallback(InstanceId, _OnWindowCloseRequested);

            WebViewWindows ret = new WebViewWindows(InstanceId);
            ret._Width = width;
            ret._Height = height;
            _WebViewDict.Add(InstanceId, ret);

            return ret;
        }

        public void PostMessage(string message, bool isJSON = false)
        {
            WebViewNative.PostWebMessage(this._InstId, message, isJSON);
        }

        public void InvokeScript(string script)
        {
            WebViewNative.InvokeScript(this._InstId, script);
        }

        public void Navigate(string url)
        {
            this.Url = url;
            WebViewNative.SetWebViewUrl(this._InstId, url);
        }

        public void LoadHTMLContent(string htmlContent)
        {
            WebViewNative.LoadHTMLContent(this._InstId, htmlContent);
        }

        public void Resize(int width, int height)
        {
            _Width = width;
            _Height = height;
            _ActiveResizeRequestCount++;
            WebViewNative.SetWebViewSize(_InstId, _Width, _Height);
        }

        public void GoBack()
        {
            WebViewNative.GoBackOnWebView(_InstId);
        }

        public void GoForward()
        {
            WebViewNative.GoForwardOnWebView(_InstId);
        }

        public void MouseEvent(WebViewWindowsEventData eventData)
        {
            WebViewNative.HandlePointerInput(_InstId, eventData.MousePos.x, eventData.MousePos.y, (int)eventData.Device, (int)eventData.Type, (int)eventData.Button, (int)eventData.WheelY);
        }

        public void Dispose()
        {
            if (_InstId == IntPtr.Zero)
                return;
            IntPtr temp = _InstId;
            _InstId = IntPtr.Zero;

            _WebViewDict.Remove(temp);
            WebViewNative.DestroyWebView(temp);
        }

        #region CallBack
        [AOT.MonoPInvokeCallback(typeof(WebViewNative.UrlChangedDelegate))]
        private static void _OnUrlChangedCallback(IntPtr instanceId, string url)
        {
            UnityGameThreadContext.Post((_) =>
            {
                Debug.Log("_OnUrlChangedCallback : " + url);

                var webView = _FindWebView(instanceId);
                if (webView != null)
                {
                    webView.Url = url;
                    webView.Navigated?.Invoke(url);
                }
            }, null);
        }

        [MonoPInvokeCallback(typeof(WebViewNative.NavigationButtonStatusUpdatedDelegate))]
        private static void _OnGoBackStatusUpdated(IntPtr instanceId, bool value)
        {
            UnityGameThreadContext.Post((_) =>
            {
                Debug.Log("_OnGoBackStatusUpdated : " + value);
                var webView = _FindWebView(instanceId);
                if (webView != null)
                {
                    webView.CanGoBack = value;
                    webView.CanGoBackUpdated?.Invoke(webView.CanGoBack);
                }
            }, null);
        }

        [MonoPInvokeCallback(typeof(WebViewNative.NavigationButtonStatusUpdatedDelegate))]
        private static void _OnGoForwardUpdated(IntPtr instanceId, bool value)
        {
            UnityGameThreadContext.Post((_) =>
            {
                //Debug.Log("_OnGoForwardUpdated : " + value);

                var webView = _FindWebView(instanceId);
                if (webView != null)
                {
                    webView.CanGoForward = value;
                    webView.CanGoForwardUpdated?.Invoke(webView.CanGoForward);
                }
            }, null);

        }

        [MonoPInvokeCallback(typeof(WebViewNative.PostMessageToUnityDelegate))]
        private static void _OnPostMessageCallback(IntPtr instanceId, string message)
        {
            UnityGameThreadContext.Post((_) =>
            {
                //Debug.Log("_OnPostMessageCallback : " + message);
                var webView = _FindWebView(instanceId);
                if (webView != null)
                {
                    webView.MessageReceived?.Invoke(message);
                }
            }, null);
        }


        [MonoPInvokeCallback(typeof(WebViewNative.OnReadyDelegate))]
        private static void _OnReadyCallback(IntPtr instanceId)
        {
            //WebViewNative.InvokeScript(instanceId, JSUnityPlayer);

            UnityGameThreadContext.Post((_) =>
            {
                //Debug.Log("_OnReadyCallback : ");
                var webView = _FindWebView(instanceId);
                if (webView != null)
                {
                    webView.Ready?.Invoke();
                }
            }, null);
        }

        [MonoPInvokeCallback(typeof(WebViewNative.TextureAvailableDelegate))]
        private static void _OnTextureAvailable(IntPtr instanceId, IntPtr texturePtr)
        {
            UnityGameThreadContext.Post((_) =>
            {
                //Debug.Log("_OnTextureAvailable : ");
                var webView = _FindWebView(instanceId);
                if (webView == null)
                    return;

                // We need this to be the case so that:
                // (1) We can access Unity GameObject methods
                // (2) We properly synchronize against resize requests.
                //Debug.Assert(SynchronizationContext.Current == UnityGameThreadContext);

                // Consume resize request.
                if (webView._ActiveResizeRequestCount >= 1)
                {
                    webView._ActiveResizeRequestCount--;
                }

                // Check if we're already asking for more resizes that will invalidate the target texture.
                if (webView._ActiveResizeRequestCount > 0)
                {
                    return;
                }

                webView._Texture = UnityEngine.Texture2D.CreateExternalTexture(webView._Width, webView._Height, UnityEngine.TextureFormat.RGBA32, false, true, texturePtr);
                webView.TextureReady?.Invoke(webView._Texture);

            }, null);
        }

        [MonoPInvokeCallback(typeof(WebViewNative.NewWindowRequestedDelegate))]
        private static void _OnNewWindowRequested(IntPtr instanceId, string uri)
        {
            UnityGameThreadContext.Post((_) =>
            {
                //Debug.Log("_OnNewWindowRequested : " + uri);
                var webView = _FindWebView(instanceId);
                webView?.NewWindowRequested?.Invoke(uri);
            }, null);
        }

        [MonoPInvokeCallback(typeof(WebViewNative.WindowCloseRequestedDelegate))]
        private static void _OnWindowCloseRequested(IntPtr instanceId)
        {
            UnityGameThreadContext.Post((_) =>
            {
                //Debug.Log("_OnWindowCloseRequested : ");
                var webView = _FindWebView(instanceId);
                webView?.WindowCloseRequested?.Invoke();
            }, null);
        }
        private static WebViewWindows _FindWebView(IntPtr instanceId)
        {
            _WebViewDict.TryGetValue(instanceId, out var webView);
            return webView;
        }
        #endregion
    }

    public struct WebViewWindowsEventData
    {
        private const int scrollSpeed = -12;

        public enum DeviceType
        {
            Mouse = 1,
            // Pointer or ray-based controller
            Pointer = 2,
        }

        public enum EventType
        {
            MouseMove = 0,
            MouseDown = 1,
            MouseUp = 2,
            MouseWheel = 3,
        }

        public enum MouseButton
        {
            ButtonNone = -1,
            ButtonLeft = 0,
            ButtonMiddle = 1,
            ButtonRight = 2,
        }

        public enum WheelBehaviorHint
        {
            RelativeAndButtonDown = 0,
            Absolute = 1
        }

        public MouseButton Button { get; set; }

        public EventType Type { get; set; }

        public DeviceType Device { get; set; }

        public float WheelY { get; set; }

        public Vector2Int MousePos { get; set; }

        public static WebViewWindowsEventData CreateMouseEvent(UnityEngine.Event eventData)
        {
            WebViewWindowsEventData data = new WebViewWindowsEventData();

            data.Device = DeviceType.Mouse;
            data.WheelY = (eventData.delta.y * scrollSpeed);

            switch (eventData.button)
            {
                case 0:
                    data.Button = MouseButton.ButtonLeft;
                    break;
                case 1:
                    data.Button = MouseButton.ButtonRight;
                    break;

                default:
                    data.Button = MouseButton.ButtonNone;
                    break;
            }

            switch (eventData.type)
            {
                case UnityEngine.EventType.MouseDown:
                    data.Type = EventType.MouseDown;
                    break;
                case UnityEngine.EventType.MouseUp:
                    data.Type = EventType.MouseUp;
                    break;
                case UnityEngine.EventType.MouseMove:
                    data.Type = EventType.MouseMove;
                    break;
                case UnityEngine.EventType.ScrollWheel:
                    data.Type = EventType.MouseWheel;
                    break;
            }

            return data;
        }

        public static WebViewWindowsEventData CreateMouseEvent(UnityEngine.EventSystems.PointerEventData eventData, UnityEngine.EventType eventType)
        {
            WebViewWindowsEventData data = new WebViewWindowsEventData();
            data.Device = DeviceType.Mouse;
            data.WheelY = (eventData.scrollDelta.y * scrollSpeed);

            switch (eventData.button)
            {
                default:
                    data.Button = MouseButton.ButtonNone;
                    break;
                case UnityEngine.EventSystems.PointerEventData.InputButton.Left:
                    data.Button = MouseButton.ButtonLeft;
                    break;
                case UnityEngine.EventSystems.PointerEventData.InputButton.Right:
                    data.Button = MouseButton.ButtonRight;
                    break;
                case UnityEngine.EventSystems.PointerEventData.InputButton.Middle:
                    data.Button = MouseButton.ButtonMiddle;
                    break;
            }

            switch (eventType)
            {
                case UnityEngine.EventType.MouseDown:
                    data.Type = EventType.MouseDown;
                    break;
                case UnityEngine.EventType.MouseUp:
                    data.Type = EventType.MouseUp;
                    break;
                case UnityEngine.EventType.MouseMove:
                    data.Type = EventType.MouseMove;
                    break;
                case UnityEngine.EventType.ScrollWheel:
                    data.Type = EventType.MouseWheel;
                    break;
            }
            return data;
        }
    }

}
