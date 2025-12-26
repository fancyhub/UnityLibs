/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
namespace FH.WV
{
    //TODO: Higher unity version, can use Microsoft.Web.WebView2.Core.dll directly, it use the async 
    internal class PlatformWebViewMgr_Windows : IPlatformWebViewMgr
    {

        private static class Cpp
        {
            public const int LogLevel_Debug = 0;
            public const int LogLevel_Info = 1;
            public const int LogLevel_Warn = 2;
            public const int LogLevel_Error = 3;
            public const int LogLevel_Off = 4;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void WebViewMessageCallBack(int webViewId, [MarshalAs(UnmanagedType.BStr)] string message);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void WebViewJsLogCallBack(int webViewId, [MarshalAs(UnmanagedType.BStr)] string message);
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void WebViewInnerLogCallBack(int logLevel, [MarshalAs(UnmanagedType.BStr)] string message);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void WebViewEventCallBack(int webViewId, int eventType);

            private const string PluginName = "WebView2UnityPlugin";

            [DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebViewSetUserAgent(string userAgent);

            [DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebViewSetMessageCallback(WebViewMessageCallBack callback);

            [DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebViewSetJsLogCallBack(WebViewJsLogCallBack callback);

            [DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebViewSetInnerLogCallBack(WebViewInnerLogCallBack callback, int maxLogLevel);

            [DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebViewSetEventCallBack(WebViewEventCallBack callback);


            [DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void WebViewSetHostObjName(string hostObjName);


            [DllImport(PluginName, CharSet = CharSet.Unicode)]
            private static extern int WebViewCreate(IntPtr parentWindow, string url, float posX, float posY, float width, float height);

            [DllImport(PluginName, CharSet = CharSet.Unicode)]
            public static extern void WebViewSetBGColor(int webViewId, byte bgR, byte bgG, byte bgB, byte bgA);

            [DllImport(PluginName, CharSet = CharSet.Unicode)]
            public static extern void WebViewSetScaling(int webViewId, bool scaling);


            [DllImport(PluginName)]
            public static extern void WebViewResize(int webViewId, float posX, float posY, float width, float height);

            [DllImport(PluginName, CharSet = CharSet.Unicode)]
            public static extern void WebViewNavigate(int webViewId, string url);

            [DllImport(PluginName)]
            public static extern void WebViewReload(int webViewId);

            [DllImport(PluginName)]
            public static extern bool WebViewCanGoBack(int webViewId);

            [DllImport(PluginName)]
            public static extern void WebViewGoBack(int webViewId);

            [DllImport(PluginName)]
            public static extern bool WebViewCanGoForward(int webViewId);

            [DllImport(PluginName)]
            public static extern void WebViewGoForward(int webViewId);

            [DllImport(PluginName)]
            public static extern void WebViewSetVisible(int webViewId, bool visible);

            [DllImport(PluginName)]
            public static extern bool WebViewIsVisible(int webViewId);


            [DllImport(PluginName)]
            [return: MarshalAs(UnmanagedType.BStr)] public static extern string WebViewGetUrl(int webViewId);

            [DllImport(PluginName, CharSet = CharSet.Unicode)]
            public static extern void WebViewExecuteScript(int webViewId, string javaScript);

            [DllImport(PluginName)]
            public static extern void WebViewClose(int webViewId);
            [DllImport(PluginName)]
            public static extern void WebViewCloseAll();

            [DllImport(PluginName)]
            public static extern bool WebViewIsLoading(int webViewId);

            public static int WebViewCreate(string url, float posX, float posY, float width, float height)
            {
                var unityWindowHandle = GetUnityWindowHandle();
                if (unityWindowHandle == IntPtr.Zero)
                    return 0;

                return WebViewCreate(unityWindowHandle, url, posX, posY, width, height);
            }

            [DllImport("user32.dll")]
            private static extern IntPtr GetActiveWindow();

            private static IntPtr GetUnityWindowHandle()
            {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                return GetActiveWindow();
#else                
                return UnityEditorGameViewHelper.FindGameViewHwnd();
#endif
            }

        }

#if UNITY_EDITOR
        public static class UnityEditorGameViewHelper
        {
            private static IntPtr LastGameViewHwnd = IntPtr.Zero;
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;

                public int Width => Right - Left;
                public int Height => Bottom - Top;
            }

            private static readonly RECT EditorBorder = new RECT() { Left = 1, Right = 1, Top = 40, Bottom = 1 };

            // 获取 GameView HWND
            public static IntPtr FindGameViewHwnd()
            {
                if (LastGameViewHwnd != IntPtr.Zero)
                {
                    if (IsWindow(LastGameViewHwnd))
                        return LastGameViewHwnd;
                    LastGameViewHwnd = IntPtr.Zero;
                }

                LastGameViewHwnd = _FindGameViewHwnd(GetActiveWindow());
                WebViewLog._.Assert(LastGameViewHwnd != IntPtr.Zero, "Can not find the GameView's HWND");
                return LastGameViewHwnd;
            }

            public static Rect ReCalcRect(Rect rect)
            {
                IntPtr pWin = IntPtr.Zero;
                RECT border = EditorBorder;
                pWin = UnityEditorGameViewHelper.FindGameViewHwnd();
                border = EditorBorder;
                if (pWin == IntPtr.Zero)
                    return rect;

                if (!GetClientRect(pWin, out var fullWinRect))
                    return rect;

                float fullWinWidth = fullWinRect.Width;
                float fullWinHeight = fullWinRect.Height;

                float innerWinWidth = fullWinWidth - border.Left - border.Right;
                float innerWinHeight = fullWinHeight - border.Top - border.Bottom;


                float posX = (rect.x * innerWinWidth + border.Left) / fullWinWidth;
                float posY = (rect.y * innerWinHeight + border.Top) / fullWinHeight;
                float width = rect.width * innerWinWidth / fullWinWidth;
                float height = rect.height * innerWinHeight / fullWinHeight;

                return new Rect(posX, posY, width, height);
            }

            // 查找顶层窗口
            [DllImport("user32.dll")]
            private static extern bool IsWindow(IntPtr hwnd);

            // 查找顶层窗口
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            // 查找子窗口
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("user32.dll")]
            private static extern IntPtr GetActiveWindow();

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

            private static IntPtr _FindGameViewHwnd(IntPtr parent)
            {
                // 如果你只知道 GameView，可以直接用类名和标题
                const string className = "UnityGUIViewWndClass";
                const string windowTitle = "UnityEditor.GameView";

                IntPtr hwnd = IntPtr.Zero;
                do
                {
                    hwnd = FindWindowEx(parent, hwnd, className, windowTitle);
                    if (hwnd != IntPtr.Zero)
                        break;
                } while (hwnd != IntPtr.Zero);

                return hwnd;
            }
        }
#endif

        public PlatformWebViewMgr_Windows()
        {
            var handler = UnityWebViewHandler.Init(this);

            Cpp.WebViewSetInnerLogCallBack(_OnInnerLogCallBack, Cpp.LogLevel_Debug);
            Cpp.WebViewJsLogCallBack callback = _OnJsLogCallBack;
            Cpp.WebViewSetJsLogCallBack(callback);

            Cpp.WebViewSetMessageCallback(handler.OnWebViewMsg);
            Cpp.WebViewSetEventCallBack(handler.OnWebViewEvent);
        }

        public void SetEnv(WebViewEnv config)
        {
            if (!string.IsNullOrEmpty(config.UserAgent))
                Cpp.WebViewSetUserAgent(config.UserAgent);

            if (!string.IsNullOrEmpty(config.JavascriptHostObjName))
                Cpp.WebViewSetHostObjName(config.JavascriptHostObjName);
        }

        public int Create(string url, Rect normalizedRect)
        {
#if UNITY_EDITOR
            normalizedRect = UnityEditorGameViewHelper.ReCalcRect(normalizedRect);
#endif
            int webviewId = Cpp.WebViewCreate(url, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
            Debug.Log($"=====Open: {url}, {normalizedRect}, {webviewId}");
            return webviewId;
        }

        public void SetScaling(int webViewId, bool scaling)
        {
            Cpp.WebViewSetScaling(webViewId, scaling);
        }

        public void SetBGColor(int webViewId, Color32 color)
        {
            Cpp.WebViewSetBGColor(webViewId, color.r, color.g, color.b, color.a);
        }

        public void Resize(int webViewId, Rect normalizedRect)
        {
#if UNITY_EDITOR
            normalizedRect = UnityEditorGameViewHelper.ReCalcRect(normalizedRect);
#endif
            Cpp.WebViewResize(webViewId, normalizedRect.x, normalizedRect.y, normalizedRect.width, normalizedRect.height);
        }

        public void Close(int webViewId)
        {
            Cpp.WebViewClose(webViewId);
        }

        private IPlatformWebViewMgr.OnWebViewEvent _EventCallBack;
        public void SetEventCallBack(IPlatformWebViewMgr.OnWebViewEvent eventCallBack)
        {
            _EventCallBack = eventCallBack;
        }

        public void OnEvent(int webViewId, int eventType)
        {
            switch (eventType)
            {
                case 1:
                    _EventCallBack?.Invoke(webViewId, EWebViewEventType.DocumentReady);
                    break;

                case 2:
                    _EventCallBack?.Invoke(webViewId, EWebViewEventType.Destroyed);
                    break;

                default:
                    WebViewLog._.E("unkown event type {0}", eventType);
                    break;
            }
        }

        public void CloseAll()
        {
            Cpp.WebViewCloseAll();
        }

        public void Navigate(int webViewId, string url)
        {
            Cpp.WebViewNavigate(webViewId, url);
        }

        public void Reload(int webViewId)
        {
            Cpp.WebViewReload(webViewId);
        }

        public void RunJavaScript(int webViewId, string jsCode)
        {
            Cpp.WebViewExecuteScript(webViewId, jsCode);
        }

        public string GetURL(int webViewId)
        {
            return Cpp.WebViewGetUrl(webViewId);
        }

        public bool CanGoBack(int webViewId)
        {
            return Cpp.WebViewCanGoBack(webViewId);
        }

        public void GoBack(int webViewId)
        {
            Cpp.WebViewGoBack(webViewId);
        }

        public bool CanGoForward(int webViewId)
        {
            return Cpp.WebViewCanGoForward(webViewId);
        }

        public void GoForward(int webViewId)
        {
            Cpp.WebViewGoForward(webViewId);
        }

        public void SetVisible(int webViewId, bool visible)
        {
            Cpp.WebViewSetVisible(webViewId, visible);
        }

        public bool IsVisible(int webViewId)
        {
            return Cpp.WebViewIsVisible(webViewId);
        }

        public bool CanClearCache()
        {
            return false;
        }

        public bool CanClearCookies()
        {
            return false;
        }

        public void ClearCache()
        {
        }

        public void ClearCookies()
        {
        }
        public void DeleteLocalStorage()
        {
        }

        public void OnMessage(int webViewId, string msg)
        {
            Debug.Log($"{msg}");
        }

        public void Fix()
        {
            Application.OpenURL("https://winstall.app/apps/Microsoft.EdgeWebView2Runtime");
        }

        [System.Serializable]
        public struct JsLogCallFrame
        {
            public int columnNumber;
            public string functionName;
            public int lineNumber;
            public string scriptId;
            public string url;
        }

        [System.Serializable]
        public struct JsLogStackTrace
        {
            public List<JsLogCallFrame> callFrames;
        }

        [System.Serializable]
        public struct JsLogArgument
        {
            public string type; // e.g., "string", "number", "object"
            public string value; // 实际的消息内容
        }

        // 4. 对应整个顶层 JSON 对象
        [System.Serializable]
        public struct JsLogData
        {
            public string type; // e.g., "log", "warning", "error"
            public List<JsLogArgument> arguments;
            public int executionContextId;
            public JsLogStackTrace stackTrace;
            public double timestamp;
        }


        private static void _OnJsLogCallBack(int webViewId, string msg)
        {
            try
            {
                JsLogData data = UnityEngine.JsonUtility.FromJson<JsLogData>(msg);
                string messageContent = "N/A";
                if (data.arguments != null && data.arguments.Count > 0)
                    messageContent = data.arguments[0].value ?? "[Empty Message Value]";

                string sourceUrl = "Unknown Source";
                if (data.stackTrace.callFrames != null && data.stackTrace.callFrames.Count > 0)
                    sourceUrl = data.stackTrace.callFrames[0].url;

                switch (data.type)
                {
                    case "debug":
                        WebViewLog._.D("JSLog, WebviewId: {0}, {1} @ {2} -> {3}", webViewId, data.type, sourceUrl, messageContent);
                        break;

                    case "log":
                    case "info":
                        WebViewLog._.I("JSLog, WebviewId: {0}, {1} @ {2} -> {3}", webViewId, data.type, sourceUrl, messageContent);
                        break;

                    case "warning":
                        WebViewLog._.W("JSLog, WebviewId: {0}, {1} @ {2} -> {3}", webViewId, data.type, sourceUrl, messageContent);
                        break;

                    case "error":
                    case "assert":
                        WebViewLog._.E("JSLog, WebviewId: {0}, {1} @ {2} -> {3}", webViewId, data.type, sourceUrl, messageContent);
                        break;

                    default:
                        WebViewLog._.E("JSLog, WebviewId: {0}, {1} @ {2} -> {3}", webViewId, data.type, sourceUrl, messageContent);
                        break;
                }
            }
            catch (Exception e)
            {
                WebViewLog._.E(e);
            }
        }

        private static void _OnInnerLogCallBack(int lvl, string msg)
        {
            switch (lvl)
            {
                case Cpp.LogLevel_Debug:
                    WebViewLog._.D(msg);
                    break;

                case Cpp.LogLevel_Info:
                    WebViewLog._.I(msg);
                    break;

                case Cpp.LogLevel_Warn:
                    WebViewLog._.W(msg);
                    break;

                default:
                case Cpp.LogLevel_Error:
                    WebViewLog._.E(msg);
                    break;
            }
        }

        public bool IsLoading(int webViewId)
        {
            return Cpp.WebViewIsLoading(webViewId);
        }


    }
    //*/
}

#endif