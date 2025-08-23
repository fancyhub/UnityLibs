using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;
using WebViewImplement = FH.WV.WebView_Android;

namespace FH
{
    [Serializable]
    public struct WebViewConfig
    {
        public string JSName;
        public string UserAgentName;
        public string UnityHandlerName;
        public bool UseMediaManipulationOnHideAndShowByJavaScript;
    }

    [Serializable]
    public struct WebViewMsg
    {
        public int WebViewId;
        public string MethodName;
        public string Parameters;
    }

    [Serializable]
    public class WebViewParameters
    {
        public bool Scaling;
        public bool UseCookie;
        public bool DeferredDisplay;
        public bool AutoPlayMedia;
        public int BGColor;

        public string IOSApplicationNameForUserAgent;
        public bool AndroidExtraLog;
    }

    internal static class WebViewDef
    {
        public const string HandlerName = "UnityWebViewHandler";
        public const string JSName = "FH";
    }

    internal interface IWebView
    {
        public void Init(WebViewConfig config);

        public int Open(string url, Rect pos, WebViewParameters parameters);
        public void Close(int webViewId);
        public string RunJavaScript(int webViewId, string jsCode, string callback, string id);


        public string GetURL(int webViewId);
        public void Reload(int webViewId);

        //public void CanGoBackward(int webViewId);
        //public void CanGoForward(int webViewId);
        public void GoBackward(int webViewId);
        public void GoForward(int webViewId);

        public float GetLoadingProgress(int webViewId);
        public bool IsLoading(int webViewId);

        public void CloseAll();

        public bool CanClearCookies();
        public void ClearCookies();
        public bool CanClearCache();
        public void ClearCache();

        public void DeleteLocalStorage();

        public void Show(int webViewId);
        public void Hide(int webViewId);

        public void SetUserAgentString(int webViewId, string userAgentString);
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

        public static void Init()
        {
            Inst.Init(new WebViewConfig()
            {
                JSName = WebViewDef.JSName,
                UnityHandlerName = WebViewDef.HandlerName,
            });
            UnityWebViewHandler.Init();
        }

        // x, y, width, height should be normalized
        public static int Open(string url, Rect pos, WebViewParameters parameters)
        {
            return Inst.Open(url, pos, parameters);
        }

        public static void Close(int webViewId)
        {
            Inst.Close(webViewId);
        }

        public static void CloseAll()
        {
            Inst.CloseAll();
        }

        public static void Reload(int webViewId)
        {
            Inst.Reload(webViewId);
        }

        public static void SetUserAgentString(int webViewId, string userAgentString)
        {
            Inst.SetUserAgentString(webViewId, userAgentString);
        }

        public static void GoBackward(int webViewId)
        {
            Inst.GoBackward(webViewId);
        }

        public static void GoForward(int webViewId)
        {
            Inst.GoForward(webViewId);
        }

        public static string GetURL(int webViewId)
        {
            return Inst.GetURL(webViewId);
        }

        public static float GetLoadingProgress(int webViewId)
        {
            return Inst.GetLoadingProgress(webViewId);
        }

        public static bool IsLoading(int webViewId)
        {
            return Inst.IsLoading(webViewId);
        }


        public static bool CanClearCookies()
        {
            return Inst.CanClearCookies();

        }

        public static void ClearCookies()
        {
            Inst.ClearCookies();
        }

        public static bool CanClearCache()
        {
            return Inst.CanClearCache();
        }

        public static void ClearCache()
        {
            Inst.ClearCache();
        }

        public static void DeleteLocalStorage()
        {
            Inst.DeleteLocalStorage();
        }

        public static void ShowWebView(int webViewId)
        {
            Inst.Show(webViewId);
        }

        public static void HideWebView(int webViewId)
        {
            Inst.Hide(webViewId);
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

            try
            {
                WebViewLog._.D(content);

                var msg = UnityEngine.JsonUtility.FromJson<WebViewMsg>(content);
            }
            catch (Exception e)
            {
                WebViewLog._.E(e);
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

