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
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.ConsoleMessage;
import android.webkit.CookieManager;
import android.webkit.JavascriptInterface;
import android.webkit.ValueCallback;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.widget.FrameLayout;

import com.unity3d.player.UnityPlayer;

import java.util.List;


class MyWebViewJavaScriptHandler
{
    private final  MyWebViewData _Data;
    public MyWebViewJavaScriptHandler(MyWebViewData data)
    {
        _Data = data;
    }

//    @JavascriptInterface
//    public void on_HTML5_Video_Ended()
//    {
//        WebViewManager.RunOnUiThread(new Runnable() {
//            @Override
//            public void run() {
//                WebViewStatus status = getStatus(_webViewId);
//                if(status == null || status.WebChromeClient == null) return;
//                status.WebChromeClient.onHideCustomView();
//            }
//        });
//    }

    @JavascriptInterface
    public void postMessage(String data)
    {
        _Data.OnJsMsg(data);
    }
}
