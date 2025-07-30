package com.github.fancyhub.webview;

import android.webkit.JavascriptInterface;

class WebViewJavaScriptHandler
{
    private final int _webViewId;
    public WebViewJavaScriptHandler(int webViewId)
    {
        _webViewId = webViewId;
    }

    @JavascriptInterface
    public void on_HTML5_Video_Ended()
    {
        WebViewManager.RunOnUiThread(new Runnable() {
            @Override
            public void run() {
                WebViewStatus status = WebViewManager._GetWebViewStatus(_webViewId);
                if(status == null || status.WebChromeClient == null) return;
                status.WebChromeClient.onHideCustomView();
            }
        });
    }

    @JavascriptInterface
    public void postMessage(String data)
    {
        WebViewManager.CallUnity(_webViewId, "OnJavaScriptPostMessage", data);
    }
}
