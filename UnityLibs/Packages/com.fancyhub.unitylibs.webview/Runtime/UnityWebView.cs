/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


#if UNITY_EDITOR 
    #if UNITY_STANDALONE_WIN
        using WebViewImplement = FH.WV.WebView_Windows;
#elif UNITY_STANDALONE_MAC
        using WebViewImplement = FH.WV.WebView_IOS;
#else
        using WebViewImplement = FH.WV.WebView_Empty;
#endif
#elif UNITY_STANDALONE_WIN
    using WebViewImplement = FH.WV.WebView_Windows;
#elif UNITY_ANDROID
    using WebViewImplement = FH.WV.WebView_Android;
#elif UNITY_IOS
    using WebViewImplement = FH.WV.WebView_IOS;
#else 
    using WebViewImplement = FH.WV.WebView_Empty;
#endif

namespace FH
{
    [Serializable]
    public struct WebViewEnv
    {
        public string JavascriptHostObjName;
        public string UserAgent;
        public bool UseMediaManipulationOnHideAndShowByJavaScript;
    }

    [Serializable]
    public class WebViewParameters
    {
        public bool Scaling;
        public Color32 BGColor;
    }

    internal static class WebViewDef
    {
        public const string HandlerName = "UnityWebViewHandler";
        public const string JsHostObjName = "FH";
    }

    public interface IWebView
    {
        public void SetEnv(WebViewEnv config);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="normalizedRect">normalized rect, [0,1], LeftTop is (0,0), RightBottom is (1,1)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int Open(string url, Rect normalizedRect, WebViewParameters parameters);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="normalizedRect">normalized rect, [0,1], LeftTop is (0,0), RightBottom is (1,1)</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public void Resize(int webViewId, Rect normalizedRect);
        public void Close(int webViewId);
        public void RunJavaScript(int webViewId, string jsCode);

        public string GetURL(int webViewId);
        public void Reload(int webViewId);

        public void GoBack(int webViewId);
        public void GoForward(int webViewId);
        public bool IsLoading(int webViewId);

        public void SetVisible(int webViewId, bool visible);
        public bool IsVisible(int webViewId);

        public void CloseAll();
    }

    public static class UnityWebView
    {
        private static IWebView _;
        private static IWebView Inst
        {
            get
            {
                if (_ != null)
                    return _;
                _ = new WebViewImplement();
                return _;
            }
        }

        private static bool _HasSetEnv = false;
        public static bool HasSetEnv()
        {
            return _HasSetEnv;
        }

        public static void SetEnv(WebViewEnv env)
        {
            _HasSetEnv = true;
            Inst.SetEnv(env);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="rect">x, y, width, height should be normalized</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int Open(string url, Rect normalizedRect, WebViewParameters parameters)
        {
            return Inst.Open(url, normalizedRect, parameters);
        }

        public static void Resize(int webViewId, Rect normalizedRect)
        {
            Inst.Resize(webViewId, normalizedRect);
        }

        public static void Close(int webViewId)
        {
            if (webViewId == 0)
                return;
            Inst.Close(webViewId);
        }

        public static void Close(ref int webViewId)
        {
            if (webViewId == 0)
                return;
            int t = webViewId;
            webViewId = 0;
            Inst.Close(t);
        }

        public static void CloseAll()
        {
            Inst.CloseAll();
        }

        public static void Reload(int webViewId)
        {
            Inst.Reload(webViewId);
        }

        public static void GoBackward(int webViewId)
        {
            Inst.GoBack(webViewId);
        }

        public static void GoForward(int webViewId)
        {
            Inst.GoForward(webViewId);
        }

        public static string GetURL(int webViewId)
        {
            return Inst.GetURL(webViewId);
        }

        public static bool IsLoading(int webViewId)
        {
            return Inst.IsLoading(webViewId);
        }

        public static void SetVisible(int webViewId, bool visible)
        {
            Inst.SetVisible(webViewId, visible);
        }
        public static bool IsVisible(int webViewId)
        {
            return Inst.IsVisible(webViewId);
        }

        public static void RunJavaScript(int webViewId, string jsCode)
        {
            Inst.RunJavaScript(webViewId, jsCode);
        }
    }

    internal class UnityWebViewHandler : MonoBehaviour
    {
        private static UnityWebViewHandler _;

        private static volatile bool _ActionFlag = false;
        private static List<Action> _ActionQueue = new List<Action>();

        private List<Action> _TempList = new List<Action>();

        public static void Init()
        {
            if (_ != null)
                return;

            GameObject handler = new GameObject();
            _ = handler.AddComponent<UnityWebViewHandler>();
            handler.name = WebViewDef.HandlerName;
            GameObject.DontDestroyOnLoad(handler);
        }

        public static void RunInUpdate(Action action)
        {
            if (action == null)
                return;

            lock (_ActionQueue)
            {
                _ActionQueue.Add(action);
                _ActionFlag = true;
            }
        }

        void Update()
        {
            if (!_ActionFlag)
                return;

            _TempList.Clear();
            lock (_ActionQueue)
            {
                _TempList.AddRange(_ActionQueue);
                _ActionQueue.Clear();
                _ActionFlag = false;
            }

            foreach (Action action in _TempList)
            {
                action();
            }
            _TempList.Clear();
        }

        public void OnWebViewMsg(string content)
        {
            if (string.IsNullOrEmpty(content))
                return;
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

