/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/12/1
 * Title   :
 * Desc    :
 *************************************************************************************/

package com.fancyhub.webview;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ResolveInfo;
import android.net.Uri;
import android.os.Build;
import android.util.Log;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.ConsoleMessage;
import android.webkit.CookieManager;
import android.webkit.ValueCallback;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.widget.FrameLayout;

import com.unity3d.player.UnityPlayer;

import java.util.List;


class WebViewSharedData {
    public final Activity UnityActivity;
    public final IWebViewManagerCallBack WebViewManagerCallBack;

    public final String NameInJavaScript;

    private FrameLayout _UnityActivityFrameLayout;


    public final  ValueBox<Integer> GlobalGBColor;
    public final ValueBox<String> GlobalUserAgent;
    public final ValueBox<Boolean> GlobalScaling;


    public WebViewSharedData(String nameInJavascript, IWebViewManagerCallBack callBack) {
        GlobalGBColor = new ValueBox<>();
        GlobalUserAgent=new ValueBox<>();
        GlobalScaling = new ValueBox<>();

        this.NameInJavaScript= nameInJavascript;
        this.UnityActivity = UnityPlayer.currentActivity;
        this.WebViewManagerCallBack = callBack;
        UnityActivity.runOnUiThread(() ->
        {
            _UnityActivityFrameLayout = new FrameLayout(UnityActivity);
            UnityActivity.addContentView(_UnityActivityFrameLayout, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
            _UnityActivityFrameLayout.setVisibility(View.INVISIBLE);
        });
    }

    public FrameLayout GetLayout() {
        return _UnityActivityFrameLayout;
    }

    public Context GetContext() {
        return UnityActivity;
    }

    public void RunOnUiThread(Runnable runnable) {
        UnityActivity.runOnUiThread(runnable);
    }

    public IWebViewManagerCallBack GetWebViewCallback()
    {
        return WebViewManagerCallBack;
    }

    public void Debug(String msg) {
        Log.d(WebViewDefine.LogTag,msg);
        WebViewManagerCallBack.OnInternalLog(WebViewDefine.LOG_LEVEL_Debug, msg);
    }

    public void Info(String msg) {
        Log.i(WebViewDefine.LogTag,msg);
        WebViewManagerCallBack.OnInternalLog(WebViewDefine.LOG_LEVEL_Info, msg);
    }

    public void Warn(String msg) {
        Log.w(WebViewDefine.LogTag,msg);
        WebViewManagerCallBack.OnInternalLog(WebViewDefine.LOG_LEVEL_WARNING, msg);
    }

    public void Error(String msg) {
        Log.e(WebViewDefine.LogTag,msg);
        WebViewManagerCallBack.OnInternalLog(WebViewDefine.LOG_LEVEL_ERROR, msg);
    }
}



enum EWebViewEvent
{
    DocumentReady,
    Destroy,
}

class MyWebViewData {

    public final  int WebViewId;

    public final WebViewSharedData SharedData;

    public String URL;
    public boolean IsLoading;

    public int LoadProgress;

    public final  ValueBox<Integer> BGColor;
    public final ValueBox<Boolean> Scaling;

    public MyWebViewData(int webViewId, WebViewSharedData sharedData) {
        this.WebViewId =webViewId;
        SharedData = sharedData;
        BGColor = new ValueBox<>();
        Scaling =new ValueBox<>();
    }

    public void OnJsLog(ConsoleMessage consoleMsg) {
        ConsoleMessage.MessageLevel msgLvl = consoleMsg.messageLevel();
        String source = consoleMsg.sourceId();
        int lineNumber = consoleMsg.lineNumber();
        String msg = consoleMsg.message();

        SharedData.WebViewManagerCallBack.OnJsLog(WebViewId, msgLvl.toString(), source, lineNumber, msg);
    }


    public void RequestIntent(String url)
    {
        Debug("RequestIntent: " + url);
        if (url.startsWith("intent://"))
        {
            try
            {
                Intent intent = Intent.parseUri(url, Intent.URI_INTENT_SCHEME);
                intent.addCategory(Intent.CATEGORY_BROWSABLE);
                intent.setComponent(null);
                if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.ICE_CREAM_SANDWICH_MR1)
                {
                    intent.setSelector(null);
                }
                List<ResolveInfo> resolves = SharedData.UnityActivity.getPackageManager().queryIntentActivities(intent, 0);
                if(resolves != null && resolves.size() > 0)
                {
                    if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN)
                    {
                        SharedData.UnityActivity.startActivityIfNeeded(intent, WebViewManager.WEB_VIEW_INTENT_REQUEST_CODE);
                    }
                    else
                    {
                        SharedData.UnityActivity.startActivityForResult(intent, WebViewManager.WEB_VIEW_INTENT_REQUEST_CODE);
                    }
                }
            }
            catch(Throwable t)
            {
                Error("RequestIntent: " + url +" : " +t);
            }
        }
        else
        {
            try
            {
                Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
                intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_SINGLE_TOP);
                SharedData.UnityActivity.startActivity(intent);
            }
            catch(Throwable t)
            {
                Error("RequestIntent: " + url + " : "+t);
            }
        }
    }



    public void Debug(String msg) {
        msg = "WebViewId: " + WebViewId + " " + msg;
        Log.d(WebViewDefine.LogTag, msg);
        SharedData.WebViewManagerCallBack.OnInternalLog(WebViewDefine.LOG_LEVEL_Debug, msg);
    }

    public void Info(String msg) {
        msg = "WebViewId: " + WebViewId + " " + msg;
        Log.i(WebViewDefine.LogTag, msg);
        SharedData.WebViewManagerCallBack.OnInternalLog(WebViewDefine.LOG_LEVEL_Info, msg);
    }

    public void Warn(String msg) {
        msg = "WebViewId: " + WebViewId + " " + msg;
        Log.w(WebViewDefine.LogTag, msg);
        SharedData.WebViewManagerCallBack.OnInternalLog(WebViewDefine.LOG_LEVEL_WARNING, msg);
    }

    public void Error(String msg) {
        msg = "WebViewId: " + WebViewId + " " + msg;
        Log.e(WebViewDefine.LogTag, msg);
        SharedData.WebViewManagerCallBack.OnInternalLog(WebViewDefine.LOG_LEVEL_ERROR, msg);
    }

    public void OnJsMsg(String msg)
    {
        SharedData.WebViewManagerCallBack.OnJsMessage(WebViewId,msg);
    }
    public void OnWebViewEvent(EWebViewEvent eventType ) {
         final int EventType_Ready = 1;
         final int EventType_Destroyed=2;

        switch (eventType)
        {
            case DocumentReady:
                SharedData.WebViewManagerCallBack.OnEvent(WebViewId, EventType_Ready);
            break;

            case Destroy:
                SharedData.WebViewManagerCallBack.OnEvent(WebViewId, EventType_Destroyed);
                break;
        }
    }
}

class MyWebView {
    private static volatile int _WebViewIdGen = 0;

    private static synchronized int _NewWebViewId() {
        _WebViewIdGen++;
        int id = _WebViewIdGen;
        return id;
    }

    private final  int _webViweId;

    private WebViewSharedData _SharedData;

    private MyWebViewData _WebViewData;

    private WebView _WebView;
    private WebSettings _WebViewSettings;
    private MyWebViewClient _WebViewClient;
    public MyWebChromeClient _WebChromeClient;
    private MyWebViewJavaScriptHandler _WebViewJavaScriptHandler;

    public MyWebView(WebViewSharedData sharedData) {
        _SharedData = sharedData;
        _webViweId = _NewWebViewId();
        _WebViewData = new MyWebViewData(_webViweId, _SharedData);
    }

    public int GetId() {
        return _webViweId;
    }

    //In UI Thread
    public void Create(String url, float x, float y, float width, float height) {
        if (this._WebView != null)
            return;

        try {
            this._WebView = new WebView(_SharedData.GetContext());
            this._WebViewSettings = this._WebView.getSettings();

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB) {
                this._WebView.setLayerType(View.LAYER_TYPE_HARDWARE, null);
            }

            if(_SharedData.GlobalUserAgent.HasValue()){
                _WebViewSettings.setUserAgentString(_SharedData.GlobalUserAgent.GetValue());
            }

            FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WRAP_CONTENT, FrameLayout.LayoutParams.WRAP_CONTENT);
            layoutParams.gravity = Gravity.LEFT | Gravity.BOTTOM;
            _CalcLayout(x,y,width,height, layoutParams);
            _SharedData.GetLayout().addView(_WebView, layoutParams);
            _SharedData.GetLayout().setVisibility(View.VISIBLE);
            this._WebView.setVisibility(View.VISIBLE);
            this._WebView.requestFocus();

            boolean useCookie = true;
            CookieManager.getInstance().setAcceptCookie(useCookie);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                CookieManager.getInstance().setAcceptThirdPartyCookies(this._WebView, useCookie);
            }

            this._WebViewSettings.setJavaScriptEnabled(true);

            this._WebViewSettings.setDisplayZoomControls(false);
            this._WebViewSettings.setDomStorageEnabled(true);


            if (_SharedData.NameInJavaScript != null && _SharedData.NameInJavaScript.length() > 0) {
                this._WebViewJavaScriptHandler = new MyWebViewJavaScriptHandler(_WebViewData);
                this._WebView.addJavascriptInterface(this._WebViewJavaScriptHandler, _SharedData.NameInJavaScript);
            }

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                this._WebViewSettings.setMixedContentMode(WebSettings.MIXED_CONTENT_ALWAYS_ALLOW);
            }

            {
                boolean hasBGColor=false;
                int bgColor = 0;
                if(_SharedData.GlobalGBColor.HasValue()){
                    hasBGColor=true;
                    bgColor = _SharedData.GlobalGBColor.GetValue();
                }
                if(_WebViewData.BGColor.HasValue()){
                    hasBGColor=true;
                    bgColor = _WebViewData.BGColor.GetValue();
                }

                if(hasBGColor){
                    _WebView.setBackgroundColor(bgColor);
                }
            }

            {
                boolean hasScaling=false;
                boolean scaling = false;
                if(_SharedData.GlobalScaling.HasValue()){
                    hasScaling=true;
                    scaling = _SharedData.GlobalScaling.GetValue();
                }
                if(_WebViewData.Scaling.HasValue()){
                    hasScaling=true;
                    scaling = _WebViewData.Scaling.GetValue();
                }

                if(hasScaling){
                    _WebViewSettings.setSupportZoom(scaling);
                    _WebViewSettings.setBuiltInZoomControls(scaling);
                }
            }

            this._WebViewClient = new MyWebViewClient(_WebViewData);
            _WebView.setWebViewClient(this._WebViewClient);

            _WebChromeClient = new MyWebChromeClient(_WebViewData);
            this._WebView.setWebChromeClient(_WebChromeClient);

            this._WebView.loadUrl(url);
        } catch (Throwable t) {
        }
    }

    public void Navigate(String url) {
        if (_WebView == null)
            return;

        _WebView.loadUrl(url);
    }

    //In UI Thread
    public void Resize(float x, float y, float width, float height) {
        if (_WebView == null)
            return;

        FrameLayout.LayoutParams layoutParams = (FrameLayout.LayoutParams) _WebView.getLayoutParams();
        _CalcLayout(x,y,width,height, layoutParams);
        _WebView.setLayoutParams(layoutParams);
    }

    //In UI Thread
    public void SetScaling(boolean scaling) {
        if (_WebView == null)
            return;
        _WebViewData.Scaling.SetValue(scaling);
        _WebViewSettings.setSupportZoom(scaling);
        _WebViewSettings.setBuiltInZoomControls(scaling);
    }

    //In UI Thread
    public void SetBGColor(int bgColor) {
        if (_WebView == null)
            return;
        _WebViewData.BGColor.SetValue(bgColor);
        _WebView.setBackgroundColor(bgColor);
    }

    public String GetUrl() {
        if (_WebView == null)
            return null;

        return _WebViewData.URL;
    }

    public boolean IsLoading() {
        if (_WebView == null)
            return false;
        _WebViewData.Debug("IsLoading: ");
        return _WebViewData.IsLoading;
    }

    public void Reload()
    {
        if(_WebView==null)
            return;
        _WebViewData.Debug("Reload: ");
        _WebView.reload();
    }

    public void GoBack() {
        if (_WebView == null)
            return;
        _WebViewData.Debug("GoBack: ");
        _WebView.goBack();
    }

    public void SetVisible(boolean visible)
    {
        if (_WebView == null)
            return;
        _WebViewData.Debug("SetVisible: "+visible);

        if(visible)
        {
            try
            {
                _WebView.onResume();
                _WebView.resumeTimers();
            }
            catch(Throwable t)
            {
                _WebViewData.Error("SetVisible: true, " + t);
            }
            _SharedData.GetLayout().setVisibility(View.VISIBLE);
            _WebView.setVisibility(View.VISIBLE);
            _WebView.requestFocus();
        }
        else
        {
            try
            {
                _WebView.onPause();
                _WebView.pauseTimers();
            }
            catch(Throwable t)
            {
                _WebViewData.Error("SetVisible: false, " + t);
            }
            _WebView.setVisibility(View.GONE);
        }
    }

    public void GoForward() {
        if (_WebView == null)
            return;
        _WebView.goForward();
    }
    public void  RunJsCode(String jsCode){
        if(_WebView==null)
            return;
        _WebViewData.Debug("RunJsCode: ");
        _WebView.evaluateJavascript(jsCode, new ValueCallback<String>() {
            @Override
            public void onReceiveValue(String value) {

            }
        });
    }
    public void Destroy() {
        if (_WebView == null)
        {
            _WebViewData.Debug("_WebView is null");
            return;
        }

        _WebViewData.Debug("Destroy: ");
        WebView webView = _WebView;
        _WebView = null;

        _SharedData.GetLayout().removeView(webView);
        webView.destroy();
    }

    private void _CalcLayout(float x, float y, float width, float height, FrameLayout.LayoutParams inoutLayoutParam) {
        float totalWidth = _SharedData.GetLayout().getWidth();
        float totalHeight = _SharedData.GetLayout().getHeight();

        inoutLayoutParam.leftMargin = (int) (x * totalWidth);
        inoutLayoutParam.topMargin = (int) (y * totalHeight);
        inoutLayoutParam.rightMargin = (int) ((1.0f - width-x) * totalWidth);
        inoutLayoutParam.bottomMargin = (int) ((1.0f - height -y) * totalHeight);
    }
}
