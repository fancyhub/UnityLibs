/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/12/1
 * Title   :
 * Desc    :
 *************************************************************************************/
package com.fancyhub.webview;

import android.view.View;
import android.webkit.ConsoleMessage;
import android.webkit.WebChromeClient;
import android.webkit.WebView;

class MyWebChromeClient extends WebChromeClient
{
    private final MyWebViewData _Data;

    public MyWebChromeClient(MyWebViewData data)
    {
        _Data = data;
    }

    @Override
    public void onProgressChanged(WebView view, int newProgress) {
        super.onProgressChanged(view, newProgress);
        _Data.LoadProgress = newProgress;
    }

    @Override
    public boolean onConsoleMessage(ConsoleMessage consoleMessage) {
        _Data.OnJsLog(consoleMessage);
        return true;
    }
    @Override
    public void onShowCustomView(View view, int requestedOrientation, CustomViewCallback callback) {
        onShowCustomView(view, callback);
    }

    @Override
    public void onShowCustomView(View view, CustomViewCallback callback) {
        _Data.Debug("onShowCustomView");
    }

    @Override
    public void onHideCustomView() {
        _Data.Debug("onHideCustomView");
    }

}
