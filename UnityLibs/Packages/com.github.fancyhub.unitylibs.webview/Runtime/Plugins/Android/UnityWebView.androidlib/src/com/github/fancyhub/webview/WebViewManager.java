package com.github.fancyhub.webview;

import android.app.Activity;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.os.Build;

import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.CookieManager;
import android.webkit.CookieSyncManager;
import android.webkit.ValueCallback;
import android.webkit.WebSettings;
import android.webkit.WebStorage;
import android.webkit.WebView;
import android.widget.FrameLayout;

import com.unity3d.player.UnityPlayer;
import com.google.gson.Gson;

import java.io.File;
import java.io.FileOutputStream;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.Map;
import java.util.Set;







public class WebViewManager {

    private static String LogTag = "UnityWebView";

    private static Activity _unityActivity;
    public static FrameLayout UnityActivityFrameLayout;
    private static IUnityMessenger _UnityMessenger;

    public static void Init(String unityObjName)
    {
        if(_unityActivity!= null && UnityActivityFrameLayout!=null)
            return;

        _UnityMessenger = new UnityMessenger(unityObjName);
        WebViewLogger._UnityMessenger=_UnityMessenger;

        _unityActivity=UnityPlayer.currentActivity;

        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if(UnityActivityFrameLayout == null)
                {
                    UnityActivityFrameLayout = new FrameLayout(_unityActivity);
                    _unityActivity.addContentView(UnityActivityFrameLayout, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
                    UnityActivityFrameLayout.setVisibility(View.INVISIBLE);
                }
            }
        });
    }


    public static void CallUnity(int webViewId, String methodName, String parameters)
    {
        _UnityMessenger.Call(webViewId,methodName,parameters);
    }

    public static Gson GsonForWebView = new Gson();

    public static Activity UnityActivity() { return _unityActivity; }

    public static boolean RunOnUiThread(Runnable r)
    {
        if(_unityActivity == null) return false;
        _unityActivity.runOnUiThread(r);
        return true;
    }

    static void _pause() {
        for (Map.Entry<Integer, WebView> entry:_WebViews.entrySet()) {
            WebView webView = entry.getValue();
            if(webView != null)
            {
                try
                {
                    webView.onPause();
                    webView.pauseTimers();
                }
                catch(Throwable t)
                {
                    WebViewLogger. Log("@_pause(): " + entry.getKey(), t);
                }
            }
        }
    }

    static void _pause(int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView != null)
        {
            try
            {
                webView.onPause();
            }
            catch(Throwable t)
            {
                WebViewLogger. Log( webViewId,"@_pause()", t);
            }
        }
    }


    public static void onActivityResume(Activity activity) {
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                _resume();
            }
        });
    }

    static void _resume() {
        for (Map.Entry<Integer, WebView> entry:_WebViews.entrySet()) {
            WebView webView = entry.getValue();
            WebViewStatus status = _GetWebViewStatus(entry.getKey());
            if(webView != null && status != null && !status.Hiding)
            {
                try
                {
                    webView.onResume();
                    webView.resumeTimers();
                }
                catch(Throwable t)
                {
                    WebViewLogger. Log("@_resume(): " + entry.getKey(), t);
                }
            }
        }
    }

    static void _resume(int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        WebViewStatus status = _GetWebViewStatus(webViewId);
        if(webView != null && status != null && !status.Hiding)
        {
            try
            {
                webView.onResume();
            }
            catch(Throwable t)
            {
                WebViewLogger. Log( webViewId,"@_resume()", t);
            }
        }
    }

    public static void destroy() {
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                _destroy();
            }
        });
    }

    static void _destroy() {
        for (Map.Entry<Integer, WebView> entry:_WebViews.entrySet()) {
            WebView webView = entry.getValue();
            if(webView != null)
            {
                try
                {
                    webView.destroy();
                }
                catch(Throwable t)
                {
                    WebViewLogger.    Log("@_destroy(): " + entry.getKey(), t);
                }
            }
        }
        _WebViews.clear();
        _WebViewStatuses.clear();
    }

    private static View _focus;
    public static final int WEB_VIEW_INTENT_REQUEST_CODE = 0x222;

    private static volatile int _NextWebViewId = 0;
    private static synchronized int NewWebViewId()
    {
        _NextWebViewId++;
        int id = _NextWebViewId;
        return id;
    };

    private static volatile HashMap<Integer, WebView> _WebViews = new HashMap<>();
    public static synchronized WebView _GetWebView(int webViewId) { return _WebViews.get(webViewId); }

    private static volatile HashMap<Integer, WebViewStatus> _WebViewStatuses = new HashMap<>();
    public static synchronized WebViewStatus _GetWebViewStatus(int webViewId) { return _WebViewStatuses.get(webViewId); }

    public static int Open(final String url, final float x, final float y, final float width, final float height, final String parameters)
    {
        if(_unityActivity == null)
            return -1;

        final int webViewId = NewWebViewId();
        WebViewStatus status = new WebViewStatus();
        status.Url = url;
        status.Opening = true;
        _WebViewStatuses.put(webViewId, status);

        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {

                _open(webViewId, url, x, y, width, height, parameters);
            }
        });

        return webViewId;
    }

    private static void _open(final int webViewId, String url, float x, float y, float width, float height, String parameters)
    {
        if(_unityActivity == null)
            return;
        WebViewStatus status = _GetWebViewStatus(webViewId);
        if(status == null)
            return;

        try
        {
            if(_WebViews.isEmpty())
            {
                _focus = _unityActivity.getCurrentFocus();
            }

            WebViewParameters webViewParameters;
            if(parameters != null && parameters.length() > 0)
            {
                webViewParameters = GsonForWebView.fromJson(parameters, WebViewParameters.class);
            }
            else
            {
                webViewParameters = new WebViewParameters();
            }


            final WebView webView = new WebView(_unityActivity);
            _WebViews.put(webViewId, webView);

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB) {
                webView.setLayerType(View.LAYER_TYPE_HARDWARE, null);
            }

            FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WRAP_CONTENT, FrameLayout.LayoutParams.WRAP_CONTENT);
            layoutParams.gravity = Gravity.LEFT | Gravity.BOTTOM;

            float totalWidth = UnityActivityFrameLayout.getWidth();
            float totalHeight = UnityActivityFrameLayout.getHeight();


            layoutParams.leftMargin = (int)(x * totalWidth);
            layoutParams.bottomMargin = (int)(y * totalHeight);
            layoutParams.width = (int)(width * totalWidth);
            layoutParams.height = (int)(height * totalHeight);

            UnityActivityFrameLayout.addView(webView, layoutParams);

            WebViewLogger.Debug("@_open(): DeferredDisplay: " + webViewParameters.DeferredDisplay);
            if(webViewParameters.DeferredDisplay)
            {
                status.DeferredDisplay = true;
                status.DeferringDisplay = true;
                webView.setVisibility(View.GONE);
            }
            else
            {
                UnityActivityFrameLayout.setVisibility(View.VISIBLE);
                webView.setVisibility(View.VISIBLE);
                webView.requestFocus();
            }

            CookieManager.getInstance().setAcceptCookie(webViewParameters.UseCookie);
            if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP)
            {
                CookieManager.getInstance().setAcceptThirdPartyCookies(webView, webViewParameters.UseCookie);
            }

            webView.getSettings().setJavaScriptEnabled(true);

            webView.getSettings().setSupportZoom(webViewParameters.Scaling);
            webView.getSettings().setBuiltInZoomControls(webViewParameters.Scaling);

            webView.getSettings().setDisplayZoomControls(false);
            webView.getSettings().setDomStorageEnabled(true);

            if(Build.VERSION.SDK_INT >= 17)
            {
                webView.getSettings().setMediaPlaybackRequiresUserGesture(!webViewParameters.AutoPlayMedia);
            }

            if(_NameInJavaScript != null && _NameInJavaScript.length() > 0)
            {
                webView.addJavascriptInterface(new WebViewJavaScriptHandler(webViewId), _NameInJavaScript);
            }

            //Color bgColor = Color.valueOf(webViewParameters.BGColor);
            webView.setBackgroundColor(webViewParameters.BGColor);

            if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP)
            {
                webView.getSettings().setMixedContentMode(WebSettings.MIXED_CONTENT_ALWAYS_ALLOW);
            }

            webView.setWebViewClient(new UnityWebViewClient(webViewId,webViewParameters.BGColor));

            status.WebChromeClient = new UnityWebChromeClient(webViewId);
            webView.setWebChromeClient(status.WebChromeClient);

            webView.loadUrl(url);
        }
        catch (Throwable t)
        {
            WebViewLogger.Log(webViewId,"@_open()", t);
        }

        status.Opening = false;
    }


    public static boolean onBackPressed()
    {
        boolean result = false;
        if(_WebViewStatuses == null || _WebViewStatuses.size() <= 0) return result;

        for (WebViewStatus status: _WebViewStatuses.values())
        {
            if(status == null) continue;
            if(status.WebChromeClient == null) continue;
            result |= status.WebChromeClient.onBackPressed();
        }
        return result;
    }

    public static void closeAll()
    {
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                _closeAll();
            }
        });
    }

    private static void _closeAll()
    {
        if(_WebViews.size() > 0)
        {
            Set<Integer> webViewIds = _WebViews.keySet();

            for (Integer webViewId: webViewIds) {
                WebView webView = _GetWebView(webViewId);
                if(webView != null)
                {
                    try
                    {
                        UnityActivityFrameLayout.removeView(webView);
                        webView.destroy();
                    }
                    catch (Throwable t)
                    {
                        WebViewLogger. Log("@_closeAll(): " + webViewId, t);
                    }
                    finally {
                        webView = null;
                    }
                }
            }
        }

        _WebViews.clear();
        _WebViewStatuses.clear();
        checkShouldRestoreFocus();
        _focus = null;
    }

    public static void close(final int webViewId)
    {
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                _close(webViewId);
            }
        });
    }

    private static void _close(int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        WebViewStatus status = _GetWebViewStatus(webViewId);
        if(status != null)
        {
            status.DeferringDisplay = false;
            status.Hiding= false;
        }
        if(webView != null)
        {
            try
            {
                _WebViews.remove(webViewId);
//                webView.stopLoading();
                UnityActivityFrameLayout.removeView(webView);
                webView.destroy();
            }
            catch (Throwable t)
            {
                WebViewLogger.Log(webViewId, "@_close()", t);
            }
            finally {
                webView = null;

                _WebViewStatuses.remove(webViewId);

                checkShouldRestoreFocus();
                if(_WebViews.isEmpty())
                {
                    _focus = null;
                }
            }
        }
    }

    public static void reload(final int webViewId)
    {
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                _reload(webViewId);
            }
        });
    }

    private static void _reload(int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView != null)
        {
            try
            {
                webView.reload();
            }
            catch(Throwable t)
            {
                WebViewLogger.Log(webViewId, "@_reload()", t);
            }
        }
    }

    public static void canGoBackward(final int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView == null) return;
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                WebView webView = _GetWebView(webViewId);
                boolean result = false;
                try
                {
                    if(webView != null)
                    {
                        result = webView.canGoBack();
                    }
                }
                catch(Throwable t)
                {
                    WebViewLogger.Log( "@canGoBackward()", t);
                }

                CallUnity(webViewId,"CanGoBackwardResult", result + "");
            }
        });
    }

    public static void canGoForward(final int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView == null) return;
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                WebView webView = _GetWebView(webViewId);
                boolean result = false;
                try
                {
                    if(webView != null)
                    {
                        result = webView.canGoForward();
                    }
                }
                catch(Throwable t)
                {
                    WebViewLogger.Log( "@canGoFoward()", t);
                }

                CallUnity(webViewId, "CanGoForwardResult", result + "");
            }
        });
    }

    public static void goBackward(final int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView == null) return;
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                _goBackward(webViewId);
            }
        });
    }

    private static void _goBackward(int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView != null && webView.canGoBack())
        {
            try
            {
                webView.goBack();
            }
            catch(Throwable t)
            {
                WebViewLogger.Log( "@_goBackward()", t);
            }
        }
    }

    public static void goForward(final int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView == null) return;
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                _goForward(webViewId);
            }
        });
    }

    private static void _goForward(int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView != null && webView.canGoForward())
        {
            try
            {
                webView.goForward();
            }
            catch(Throwable t)
            {
                WebViewLogger.Log( "@_goForward()", t);
            }
        }
    }

    public static String getURL(int webViewId)
    {
        WebViewStatus status = _GetWebViewStatus(webViewId);
        if(status != null)
        {
            return status.Url;
        }
        else
        {
            return "";
        }
    }

    public static double getLoadingProgress(int webViewId)
    {
        WebViewStatus status = _GetWebViewStatus(webViewId);
        if(status != null)
        {
            return status.LoadingProgress;
        }
        else
        {
            return -1;
        }
    }

    public static boolean isLoading(int webViewId)
    {
        WebViewStatus status = _GetWebViewStatus(webViewId);
        if(status != null)
        {
            return status.IsLoading;
        }
        else
        {
            return false;
        }
    }

    public static boolean canClearCookies()
    {
        if(_unityActivity == null) return false;
        return true;
    }

    public static void clearCookies()
    {
        if(_unityActivity == null) return;
        if(!canClearCookies()) return;

        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                CookieManager cookieManager = CookieManager.getInstance();
                try
                {

                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                        cookieManager.removeSessionCookies(null);
                        cookieManager.removeAllCookies(null);
                        cookieManager.flush();
                    }
                    else {
                        CookieSyncManager.createInstance(_unityActivity);
                        cookieManager.removeAllCookie();
                        cookieManager.removeSessionCookie();
                        CookieSyncManager.getInstance().sync();
                    }
                }
                catch(Throwable t){}
            }
        });
    }

    public static boolean canClearCache()
    {
        if(_unityActivity == null) return false;
        return true;
    }

    public static void clearCache()
    {
        if(_unityActivity == null) return;
        if(!canClearCache()) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                String result = "";
                try
                {
                    WebView tempWebView = new WebView(_unityActivity);
                    tempWebView.clearFormData();
                    tempWebView.clearHistory();
                    tempWebView.clearMatches();
                    tempWebView.clearCache(true);
                    tempWebView.destroy();
                }
                catch(Throwable t)
                {
                    WebViewLogger.Log("@clearCache()", t);
                    result = t.getMessage();
                }
                CallUnity(0,"OnClearCache", result);
            }
        });
    }

    public static void webStorage_DeleteAllData()
    {
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                try
                {
                    WebStorage.getInstance().deleteAllData();
                } catch(Throwable t){}
            }
        });
    }

    public static boolean showing(int webViewId)
    {
        if(_unityActivity == null) return false;
        WebViewStatus status = _GetWebViewStatus(webViewId);
        if(status != null)
        {
            return !status.Hiding;
        }
        return false;
    }

    public static void show(final int webViewId)
    {
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                WebViewStatus status = _GetWebViewStatus(webViewId);
                if(status != null)
                {
                    status.Hiding = false;
                    if(!status.DeferringDisplay)
                    {
                        _show(webViewId);
                    }
                }
            }
        });
    }

    static void _show(int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView == null) return;
        WebViewLogger.Debug( webViewId + "+_show()");
        _resume(webViewId);
        UnityActivityFrameLayout.setVisibility(View.VISIBLE);
        webView.setVisibility(View.VISIBLE);
        webView.requestFocus();
        WebViewLogger.Debug(webViewId + "-_show()");
    }

    public static void hide(final int webViewId)
    {
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                WebViewStatus status = _GetWebViewStatus(webViewId);
                if(status != null)
                {
                    status.Hiding = true;
                    if(!status.DeferringDisplay)
                    {
                        _hide(webViewId);
                    }
                }
            }
        });
    }

    private static void _hide(int webViewId)
    {
        WebView webView = _GetWebView(webViewId);
        if(webView == null) return;
        WebViewLogger.Debug(webViewId+ "+_hide()");
        _pause(webViewId);
        webView.setVisibility(View.GONE);
        checkShouldRestoreFocus();
        WebViewLogger.Debug(webViewId+"-_hide()");
    }

    private static void checkShouldRestoreFocus()
    {
        boolean hideFrameLayout = true;
        boolean restoreFocus = true;
        for (Map.Entry<Integer, WebView> entry: _WebViews.entrySet())
        {
            hideFrameLayout &= (entry.getValue() == null);
            restoreFocus &= (entry.getValue() == null || entry.getValue().getVisibility() != View.VISIBLE);
        }

        WebViewLogger.Debug("@checkShouldRestoreFocus(): hideFrameLayout: " + hideFrameLayout);
        WebViewLogger.Debug("@checkShouldRestoreFocus(): restoreFocus: " + restoreFocus);

        if(hideFrameLayout)
        {
            UnityActivityFrameLayout.setVisibility(View.INVISIBLE);
        }

        if(restoreFocus)
        {
            if(_focus != null) _focus.requestFocus();
        }
    }

    public static void setUserAgentString(final int webViewId, final String userAgentString)
    {
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                WebView webView = _GetWebView(webViewId);
                if(webView == null || webView.getSettings() == null) return;
                webView.getSettings().setUserAgentString(userAgentString);
            }
        });
    }

    public static void getUserAgentString(final int webViewId)
    {
        if(_unityActivity == null) return;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                String userAgent = "";
                WebView webView = _GetWebView(webViewId);
                if(webView != null)
                {
                    userAgent = webView.getSettings().getUserAgentString();
                }
                CallUnity(webViewId,"OnGetUserAgentString", userAgent);
            }
        });
    }


    private static String _NameInJavaScript;

    public static void setNameInJavaScript(String name)
    {
        _NameInJavaScript = name;
    }
    public static String getNameInJavaScript() { return _NameInJavaScript; }

    public static String runJavaScript(final int webViewId, final String code, final String callback, final String id)
    {
        if(_unityActivity == null) return "UnityActivity is null";

        WebViewLogger.Debug(webViewId+ "@runJavaScript(): " + id + ": " + callback + "\n" + code);
        WebView webView = _GetWebView(webViewId);
        WebViewStatus status = _GetWebViewStatus(webViewId);
        if(webView == null)
        {
            if(status != null && status.Opening)
            {
                WebViewLogger. Debug(webViewId+ "@runJavaScript(): queued " + id + ": " + callback);
                JavaScriptInvoke javaScriptInvoke = new JavaScriptInvoke();
                javaScriptInvoke.code = code;
                javaScriptInvoke.callback = callback;
                javaScriptInvoke.id = id;
                status.PendingJavaScriptInvokes.addLast(javaScriptInvoke);
                return "";
            }
            else
            {
                return "WebView is null";
            }
        }

        if(Build.VERSION.SDK_INT >= 19)
        {
            _unityActivity.runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    _runJavaScript(webViewId, code, callback, id);
                }
            });
            return "";
        }
        else
        {
            return "API level too low";
        }
    }

    static void _runJavaScript(final int webViewId, final String code, final String callback, final String id)
    {
        WebView webView = _GetWebView(webViewId);
        WebViewLogger.Debug(webViewId, "@_runJavaScript(): " + id + ": " + callback);
        if(webView == null)
        {
            JavaScriptResult wvjsr = new JavaScriptResult();
            wvjsr.id = id;
            wvjsr.callback = callback;
            wvjsr.error = "WebView is null";

            WebViewLogger.Debug(webViewId, wvjsr.error);
            CallUnity(webViewId, "OnJavaScriptResult", GsonForWebView.toJson(wvjsr));
            return;
        }
        if(Build.VERSION.SDK_INT >= 19)
        {
            try {
                WebViewLogger.Debug(webViewId, "?evaluateJavascript()");
                webView.evaluateJavascript(code, new ValueCallback<String>() {
                    @Override
                    public void onReceiveValue(String value) {
                        try
                        {
                            if(callback != null && callback.length() > 0)
                            {
                                JavaScriptResult wvjsr = new JavaScriptResult();
                                wvjsr.id = id;
                                wvjsr.callback = callback;
                                wvjsr.value = value;
                                WebViewLogger.Debug( "@evaluateJavascript.ValueCallback(): " + value);
                                CallUnity(webViewId,"OnJavaScriptResult", GsonForWebView.toJson(wvjsr));
                            }
                        }
                        catch(Throwable t)
                        {
                            JavaScriptResult wvjsr = new JavaScriptResult();
                            wvjsr.id = id;
                            wvjsr.callback = callback;
                            wvjsr.error = t.getMessage();
                            WebViewLogger.Debug( webViewId,"@evaluateJavascript.ValueCallback()", t);
                            CallUnity(webViewId, "OnJavaScriptResult", GsonForWebView.toJson(wvjsr));
                        }
                    }
                });
                WebViewLogger.Debug(webViewId, "!evaluateJavascript()");
            }
            catch(Throwable t)
            {
                JavaScriptResult wvjsr = new JavaScriptResult();
                wvjsr.id = id;
                wvjsr.callback = callback;
                wvjsr.error = t.getMessage();
                WebViewLogger.Debug(webViewId, "@evaluateJavascript()", t);
                CallUnity(webViewId,"OnJavaScriptResult", GsonForWebView.toJson(wvjsr));
            }
        }
        else
        {
            JavaScriptResult wvjsr = new JavaScriptResult();
            wvjsr.id = id;
            wvjsr.callback = callback;
            wvjsr.error = "API level too low: " + Build.VERSION.SDK_INT;
            WebViewLogger. Debug(webViewId, wvjsr.error);
            CallUnity(webViewId,"OnJavaScriptResult", GsonForWebView.toJson(wvjsr));
        }
    }

    public static boolean captureScreenshot(final int webViewId, final String fileName)
    {
        if(fileName == null || fileName.length() <= 0) return false;
        if(_unityActivity == null) return false;
        _unityActivity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                WebView webView = _GetWebView(webViewId);
                if(webView == null)
                {
                    CallUnity(webViewId,"OnCaptureScreenshotFailure", "");
                    return;
                }

                try
                {
                    Bitmap bitmap;
                    // webview.draw only draws the left top most viewport, which might yields black areas if web content is scrolled
                    bitmap = Bitmap.createBitmap(UnityActivityFrameLayout.getWidth(), UnityActivityFrameLayout.getHeight(), Bitmap.Config.ARGB_8888);
                    Canvas canvas = new Canvas(bitmap);
                    UnityActivityFrameLayout.draw(canvas);
                    FrameLayout.LayoutParams lp = (FrameLayout.LayoutParams) (webView.getLayoutParams());

                    // upsidedown!
                    bitmap = Bitmap.createBitmap(bitmap, lp.leftMargin, UnityActivityFrameLayout.getHeight() - lp.height - lp.bottomMargin, lp.width, lp.height);

                    String path = _unityActivity.getExternalFilesDir(null).toString();
                    File imageFile = new File(path, fileName + ".jpg");
                    FileOutputStream writeStream = new FileOutputStream(imageFile);
                    bitmap.compress(Bitmap.CompressFormat.JPEG, 100, writeStream);
                    writeStream.flush();
                    writeStream.close();

                    CallUnity(webViewId,"OnCaptureScreenshotSuccess", imageFile.getAbsolutePath());
                }
                catch (Throwable t)
                {
                    CallUnity(webViewId,"OnCaptureScreenshotFailure", "");
                }
            }
        });
        return true;
    }
}
class JavaScriptInvoke
{
    public int webViewId;
    public String code;
    public String callback;
    public String id;
}

class JavaScriptResult
{
    public String id;
    public String callback;
    public String error;
    public String value;
}

class WebViewStatus
{
    public boolean Opening = false;
    public boolean DeferredDisplay = false;
    public boolean DeferringDisplay = false;
    public boolean Hiding = false;
    public boolean IsLoading = false;
    public double LoadingProgress = 0;
    public String Url;
    public LinkedList<JavaScriptInvoke> PendingJavaScriptInvokes = new LinkedList<>();
    public UnityWebChromeClient WebChromeClient;
}

class WebViewParameters
{
    public String UnitySendMessageGameObjectName;
    public boolean Scaling = false;
    public boolean UseCookie = true;
    public boolean DeferredDisplay = false;
    public boolean AutoPlayMedia = true;
    public int BGColor;
}
